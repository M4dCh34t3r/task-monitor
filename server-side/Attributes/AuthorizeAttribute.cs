using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskMonitor.Exceptions;
using TaskMonitor.Utils;

namespace TaskMonitor.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeAttribute(string claim = "") : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                ClaimsPrincipal user = context.HttpContext.User;

                AuthorizationUtil.ValidateUser(user);
                AuthorizationUtil.ValidateClaims(user, claim);
            }
            catch (ContextResultException ex)
            {
                context.Result = ex.ToObjectResult();
            }
        }
    }
}
