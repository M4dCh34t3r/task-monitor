using Microsoft.AspNetCore.Mvc;
using TaskMonitor.DTOs;
using TaskMonitor.Services;

namespace TaskMonitor.Controllers
{
    public class ClientSetupController(IClientSetupService clientSetupService) : AjaxController
    {
        private readonly IClientSetupService _service = clientSetupService;

        /// <summary>
        /// Returns the client side setup configurations.
        /// </summary>
        /// <response code="200">Indicates that the setup was successfully retrieved.</response>
        /// <response code="500">An unexpected server error occurred while processing the request.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientSetupDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public IActionResult GetAsync() => Execute(() => Ok(_service.Read()));
    }
}
