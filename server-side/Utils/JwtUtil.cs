using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskMonitor.Models;

namespace TaskMonitor.Utils
{
    public static class JwtUtil
    {
        private const int _renovationTimeMinutes = 30;

        public static string GenerateToken(User model, string secret, int timeoutHours = 1)
        {
            JwtSecurityTokenHandler tokenHandler = new();

            ICollection<Claim> claims =
            [
                new Claim(AuthorizationUtil.UserName, model.UserName),
                new Claim(AuthorizationUtil.UserId, model.Id.ToString()),
                new Claim(AuthorizationUtil.Admin, "false", ClaimValueTypes.Boolean),
            ];
            byte[] key = Encoding.ASCII.GetBytes(secret);

            SecurityToken token = CreateToken(tokenHandler, claims, key, timeoutHours);

            return tokenHandler.WriteToken(token);
        }

        public static bool IsTokenValid(HttpContext context, string secret)
        {
            if (
                context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last()
                is not string token
            )
                return false;

            if (IsTokenExpiring(token))
            {
                token = RewriteToken(token, secret);
                context.Request.Headers.Authorization = token;
                context.Response.Headers.Authorization = token;
            }

            if (TokenValidar(token, secret) is JwtSecurityToken jwt)
            {
                ClaimsIdentity claimsIdentity = new(jwt.Claims, "default");
                context.User = new ClaimsPrincipal(claimsIdentity);
                return true;
            }

            return false;
        }

        private static SecurityToken CreateToken(
            SecurityTokenHandler tokenHandler,
            IEnumerable<Claim> claims,
            byte[] key,
            int expirationTimeHours = 1
        )
        {
            SigningCredentials signingCredentials =
                new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            DateTime expires = DateTime.UtcNow.AddHours(expirationTimeHours);
            ClaimsIdentity subject = new(claims, "default");

            return tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    SigningCredentials = signingCredentials,
                    Expires = expires,
                    Subject = subject,
                }
            );
        }

        private static bool IsTokenExpiring(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new();

            if (tokenHandler.CanReadToken(token))
            {
                TimeSpan intervalo = tokenHandler
                    .ReadJwtToken(token)
                    .ValidTo.Subtract(DateTime.UtcNow);

                return intervalo < TimeSpan.FromMinutes(_renovationTimeMinutes)
                    && intervalo > TimeSpan.Zero;
            }

            return false;
        }

        private static string RewriteToken(string token, string secret)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            JwtSecurityToken tokenOriginal = tokenHandler.ReadJwtToken(token);

            IEnumerable<Claim> claims = tokenOriginal.Claims;
            byte[] key = Encoding.ASCII.GetBytes(secret);

            var rewritenToken = CreateToken(tokenHandler, claims, key);

            return tokenHandler.WriteToken(rewritenToken);
        }

        private static SecurityToken? TokenValidar(string token, string secret)
        {
            try
            {
                byte[] key = Encoding.ASCII.GetBytes(secret);
                new JwtSecurityTokenHandler().ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                    },
                    out SecurityToken tokenValidado
                );
                return tokenValidado;
            }
            catch
            {
                return null;
            }
        }
    }
}
