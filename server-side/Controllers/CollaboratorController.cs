using Microsoft.AspNetCore.Mvc;
using TaskMonitor.Attributes;
using TaskMonitor.DTOs;
using TaskMonitor.Services;

namespace TaskMonitor.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class CollaboratorController(ICollaboratorService service) : AjaxController
    {
        private readonly ICollaboratorService _service = service;

        /// <summary>
        /// Read all collaborators.
        /// </summary>
        /// <response code="200">Indicates that the collaborators were successfully retrieved.</response>
        /// <response code="401">Access is denied due to invalid credentials.</response>
        /// <response code="500">An unexpected server error occurred while processing the request.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public async Task<IActionResult> GetAsync() =>
            await ExecuteAsync(async () => Ok(await _service.ReadAllAsync()));
    }
}
