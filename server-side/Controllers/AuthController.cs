using Microsoft.AspNetCore.Mvc;
using TaskMonitor.DTOs;
using TaskMonitor.Services;

namespace TaskMonitor.Controllers
{
    [ApiController]
    public class AuthController(IAuthService authService) : AjaxController
    {
        private readonly IAuthService _authService = authService;

        /// <summary>
        /// Authenticates a user with the provided credentials and returns a JWT token.
        /// </summary>
        /// <param name="authRequest">The authentication request containing the username and password.</param>
        /// <response code="200">Authentication was successful. Returns a JWT token in plain text format (`text/plain`).</response>
        /// <response code="401">Authentication failed due to incorrect username. The response includes a service exception in JSON format (`application/json`).</response>
        /// <response code="404">The provided username does not exist in the system. The response includes a service exception in JSON format (`application/json`).</response>
        /// <response code="500">An unexpected server error occurred while processing the request. The response includes a generic error message in JSON format (`application/json`).</response>
        [HttpPost("Login")]
        [Produces("text/plain", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized,
            Type = typeof(ServiceExceptionDTO)
        )]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceExceptionDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public async Task<IActionResult> LoginAsync(AuthRequestDTO authRequest) =>
            await ExecuteAsync(async () => Ok(await _authService.LoginAsync(authRequest)));
    }
}
