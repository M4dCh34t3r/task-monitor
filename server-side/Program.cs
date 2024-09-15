using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;
using TaskMonitor.Context;
using TaskMonitor.Middlewares;
using TaskMonitor.Services;

const string LoggerTemplate =
    "[{@t:HH:mm:ss.fff} "
    + "{#if SourceContext is not null}{SourceContext}"
    + "{#if Subcontext is not null}::{Subcontext}{#end} {#end}"
    + "{@l:u3}{#if ErrorTag is not null} {ErrorTag}{#end}"
    + "] {@m}"
    + "{#if @x is not null}{NewLine}{@x}{#end}"
    + "{#if Rest(true) <> {}}{NewLine}{Rest(true)}{#end}"
    + "{NewLine}{NewLine}";

try
{
    WebApplication app = BuildWebApp(args);

    if (HasProcessedCLI(args, app))
        return;

    if (HasInvalidConfiguration(app.Configuration))
        return;

    app.Run();
}
catch (Exception ex)
{
    if (
        ex is HostAbortedException hostAbortedEx
        && hostAbortedEx.StackTrace?.Contains("BuildWebApp") == true
    )
        Log.Warning(
            "Host aborted during configuration. If started via \"dotnet-ef\", consider ignoring this warning"
        );
    else
        Log.Fatal(ex, "Unexpected error during server startup");
}
finally
{
    Log.CloseAndFlush();
}

static WebApplication BuildWebApp(string[] args)
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    AddSerilog(builder);
    AddSentry(builder);
    AddExtraConfigurations(builder);
    AddServices(builder);

    if (builder.Environment.IsDevelopment())
        AddSwagger(builder);

    WebApplication app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI();
        app.UseSwagger();
    }
    else
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseJwtMiddleware();
    app.MapControllers();

    return app;
}

static void AddExtraConfigurations(WebApplicationBuilder builder)
{
    builder.Configuration.AddJsonFile("appsecrets.json", false, false);
}

static void AddSentry(WebApplicationBuilder builder)
{
    var fnLogger = Log.ForContext<Program>().ForContext("Subcontext", nameof(AddSentry));
    var config = builder.Configuration;

    if (builder.Environment.IsProduction())
    {
        if (config["Secrets:SentryDsn"] is string dsn)
        {
            fnLogger.Debug("Starting Setry via {dsn}", dsn);
            builder.WebHost.UseSentry(o =>
            {
                o.Dsn = dsn;
                o.TracesSampleRate = 0.25;
                o.Environment = "production";
                o.DefaultTags["instance_name"] = config["Setup:InstanceName"]!;
            });
        }
        else
            fnLogger.Debug("Sentry is not being used since the DSN wasn't specified");
    }
    else
        fnLogger.Debug("Sentry is not being used since the app is in development mode");
}

static void AddSerilog(WebApplicationBuilder builder)
{
    var isProduction = builder.Environment.IsProduction();

    var loggerConfig = new LoggerConfiguration()
        .Enrich.WithProperty("NewLine", Environment.NewLine)
        .Enrich.FromLogContext()
        .MinimumLevel.Is(LogEventLevel.Debug)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Debug)
        .WriteTo.Console(new ExpressionTemplate(LoggerTemplate, theme: TemplateTheme.Code));
    ;

    if (isProduction)
    {
        var debugLog = builder.Configuration.GetValue<bool>("Misc:DebugLog");
        var minimumLevel = debugLog ? LogEventLevel.Debug : LogEventLevel.Information;
        var logPath = Path.Combine(builder.Configuration["Paths:Logs"]!, ".txt");

        loggerConfig
            .WriteTo.File(
                new ExpressionTemplate(LoggerTemplate),
                logPath,
                restrictedToMinimumLevel: minimumLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true
            )
            .WriteTo.Sentry(
                "",
                minimumBreadcrumbLevel: LogEventLevel.Debug,
                minimumEventLevel: LogEventLevel.Error
            );
    }

    Log.Logger = loggerConfig.CreateLogger();
    builder.Host.UseSerilog();
}

static void AddServices(WebApplicationBuilder builder)
{
    builder
        .Services.AddControllers()
        .AddJsonOptions(o =>
            o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
        );
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"))
    );

    builder
        .Services.AddScoped<IAuthService, AuthService>()
        .AddScoped<IClientSetupService, ClientSetupService>()
        .AddScoped<ICollaboratorService, CollaboratorService>()
        .AddScoped<IProjectService, ProjectService>()
        .AddScoped<ITaskService, TaskService>()
        .AddScoped<ITimeTrackerService, TimeTrackerService>();
}

static void AddSwagger(WebApplicationBuilder builder) =>
    builder.Services.AddSwaggerGen(o =>
    {
        o.SwaggerDoc("v1", new() { Title = "Task Monitor API - V1", Version = "v1" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        o.IncludeXmlComments(xmlPath);

        o.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Description = "Please, insert a valid token...",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Name = "Authorization",
                BearerFormat = "JWT",
                Scheme = "Bearer",
            }
        );

        o.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    });

static bool HasProcessedCLI(string[] args, WebApplication app)
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

    if (args.Contains("--migrate"))
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var migrator = context.GetService<IMigrator>();
        var pendingMigrationNames = context.GetMigrationNames(true);

        if (pendingMigrationNames.Length > 0)
        {
            Log.ForContext("Pending", pendingMigrationNames).Information("Migranting database...");
            migrator.Migrate();
        }
        else
            Log.Information("The migrations are already applied");

        return true;
    }

    if (args.Contains("--drop"))
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dropped = context.Database.EnsureDeleted();

        Log.Information(dropped ? "Databased dropped" : "The database doesn't exist");

        return true;
    }

    return false;
}

static bool HasInvalidConfiguration(IConfiguration configuration)
{
    string[] requiredKeys =
    [
        "ConnectionStrings:MSSQL",
        "ClientSetup:Ready",
        "Secrets:Salts:JWT",
        "Secrets:Salts:PBKDF2",
    ];

    foreach (var key in requiredKeys)
        if (string.IsNullOrWhiteSpace(configuration.GetValue<string>(key)))
        {
            Log.Information($"\"{key}\" needs to be specified in the app configuration");
            return true;
        }
    return false;
}
