using Microsoft.IdentityModel.Tokens;
using TaskMonitor.Utils;

namespace TaskMonitor.Middlewares
{
    public class JwtMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Response.Headers.Authorization.IsNullOrEmpty())
                context.Response.Headers.Remove("Authorization");

            if (JwtUtil.IsTokenValid(context, configuration.GetValue<string>("Secrets:Salts:JWT")!))
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.User = new SentryUser
                    {
                        Id = context.User.FindFirst(AuthorizationUtil.UserId)!.Value,
                        Other = new Dictionary<string, string>()
                        {
                            { "Admin", context.User.FindFirst(AuthorizationUtil.Admin)!.Value },
                        },
                    };
                });
            }

            await _next(context);
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<JwtMiddleware>();
    }
}
