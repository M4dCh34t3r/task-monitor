using Microsoft.AspNetCore.Mvc;
using TaskMonitor.Attributes;
using TaskMonitor.DTOs;
using TaskMonitor.Services;

namespace TaskMonitor.Controllers
{
    /// <response code="401">Access is denied due to invalid credentials.</response>
    /// <response code="500">An unexpected server error occurred while processing the request.</response>
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(StatusCodeResult)
    )]
    public class TimeTrackerController(ITimeTrackerService service) : AjaxController
    {
        private readonly ITimeTrackerService _service = service;

        /// <summary>
        /// Read all time trackers by task ID.
        /// </summary>
        /// <response code="200">Indicates that the time trackers were successfully retrieved.</response>
        [HttpGet("Task/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.TimeTracker[]))]
        public async Task<IActionResult> GetAsync(Guid taskId, bool includeCollaborators) =>
            await ExecuteAsync(
                async () => Ok(await _service.ReadByTaskIdAsync(taskId, includeCollaborators))
            );

        /// <summary>
        /// Read all time trackers by task ID.
        /// </summary>
        /// <response code="201">Indicates that the time tracker was successfully created.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.TimeTracker))]
        public async Task<IActionResult> PostAsync(Models.TimeTracker task) =>
            await ExecuteAsync(async () =>
            {
                var timeTracker = await _service.CreateAsync(task);
                return Created($"/{timeTracker.Id}", timeTracker);
            });

        /// <summary>
        /// Ends a time tracker by it's ID.
        /// </summary>
        /// <response code="200">Indicates that the time tracker has been successfully finished.</response>
        [HttpPost("EndDate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.TimeTracker))]
        public async Task<IActionResult> PostEndDateAsync(Guid id) =>
            await ExecuteAsync(async () => Ok(await _service.SetEndDateAsync(id)));

        /// <summary>
        /// Starts a time tracker by it's ID.
        /// </summary>
        /// <response code="200">Indicates that the time tracker has been successfully started.</response>
        [HttpPost("StartDate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.TimeTracker))]
        public async Task<IActionResult> PostStartDateAsync(Guid id) =>
            await ExecuteAsync(async () => Ok(await _service.SetStartDateAsync(id)));

        /// <summary>
        /// Updates a time tracker by it's ID.
        /// </summary>
        /// <response code="200">Indicates that the time tracker has been successfully updated.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.TimeTracker))]
        public async Task<IActionResult> PutAsync(Guid id, Models.TimeTracker timeTracker) =>
            await ExecuteAsync(async () => Ok(await _service.UpdateByIdAsync(id, timeTracker)));
    }
}
