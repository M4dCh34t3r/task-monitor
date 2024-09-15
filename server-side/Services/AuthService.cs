using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskMonitor.Context;
using TaskMonitor.DTOs;
using TaskMonitor.Enums;
using TaskMonitor.Exceptions;
using TaskMonitor.Models;
using TaskMonitor.Utils;

namespace TaskMonitor.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(AuthRequestDTO authRequest);
    }

    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        private readonly AppDbContext _context = context;

        private readonly IConfiguration _configuration = configuration;

        public async Task<string> LoginAsync(AuthRequestDTO authRequest)
        {
            if (
                await _context.Users.FirstOrDefaultAsync(u => u.UserName == authRequest.UserName)
                is not User user
            )
                throw new ServiceException(
                    HttpStatusCode.NotFound,
                    ModalTheme.Warning,
                    "User not found",
                    "The specified username doesn't match any entry in the system"
                );

            if (user.DeletedAt is not null)
                throw new ServiceException(
                    HttpStatusCode.Forbidden,
                    ModalTheme.Warning,
                    "User not accessible",
                    "The specified user has been removed from the system"
                );

            if (
                user.Password
                != HashUtil.ComputePBKDF2(
                    authRequest.Password,
                    _configuration.GetValue<string>("Secrets:Salts:PBKDF2")!
                )
            )
                throw new ServiceException(
                    HttpStatusCode.Unauthorized,
                    ModalTheme.Warning,
                    "User not authenticated",
                    "Please, enter the correct password"
                );

            return JwtUtil.GenerateToken(
                user,
                _configuration.GetValue<string>("Secrets:Salts:JWT")!
            );
        }
    }
}
