using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using TaskMonitor.Exceptions;

namespace TaskMonitor.Utils
{
    public class AuthorizationUtil
    {
        private const string ExpirationTime = "exp";
        private const string IssuedAtTime = "iat";
        private const string NotBeforeTime = "nbf";

        public const string Admin = "adm";
        public const string UserId = "uid";
        public const string UserName = "unm";

        public static void ValidateUser(ClaimsPrincipal user)
        {
            if (user.Identity is not IIdentity userIdentity || !userIdentity.IsAuthenticated)
                throw new ContextResultException(
                    HttpStatusCode.Unauthorized,
                    "Action not authorized"
                );
        }

        public static void ValidateClaims(ClaimsPrincipal usuario, string claim = "")
        {
            if (
                usuario.FindFirst(ExpirationTime) is null
                || usuario.FindFirst(IssuedAtTime) is null
                || usuario.FindFirst(NotBeforeTime) is null
                || usuario.FindFirst(NotBeforeTime) is null
                || usuario.FindFirst(UserName) is null
                || usuario.FindFirst(Admin) is not Claim adm
                || usuario.FindFirst(UserId) is not Claim uid
            )
                throw new ContextResultException(HttpStatusCode.BadRequest, "Invalid JWT claims");

            if (!Guid.TryParse(uid.Value, out _))
                throw new ContextResultException(HttpStatusCode.BadRequest, "Invalid user");

            if (string.Equals(claim, Admin) && bool.Parse(adm.Value) == false)
                throw new ContextResultException(HttpStatusCode.Forbidden, "Action not allowed");
        }
    }
}
