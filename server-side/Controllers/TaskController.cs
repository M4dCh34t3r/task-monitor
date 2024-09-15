using Microsoft.AspNetCore.Mvc;
using TaskMonitor.Attributes;
using TaskMonitor.DTOs;
using TaskMonitor.Services;

namespace TaskMonitor.Controllers
{
    /// <response code="401">Access is denied due to invalid credentials.</response>
    /// <response code="500">An unexpected server error occurred while processing the request.</response>
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
    [ProducesResponseType(
        StatusCodes.Status500InternalServerError,
        Type = typeof(StatusCodeResult)
    )]
    public class TaskController(ITaskService taskService) : AjaxController
    {
        private readonly ITaskService _service = taskService;

        /// <summary>
        /// Read all tasks.
        /// </summary>
        /// <response code="200">Indicates that the tasks were successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public async Task<IActionResult> GetAsync(bool includeProjects) =>
            await ExecuteAsync(async () => Ok(await _service.ReadAllAsync(includeProjects)));

        /// <summary>
        /// Read tasks by project ID and/or collaborator ID.
        /// </summary>
        /// <response code="200">Indicates that the tasks were successfully retrieved.</response>
        [HttpGet("Filter")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public async Task<IActionResult> GetFilterAsync(
            Guid? projectId,
            Guid? collaboratorId,
            bool includeProjects
        ) =>
            await ExecuteAsync(
                async () =>
                    Ok(await _service.FilterByIdsAsync(projectId, collaboratorId, includeProjects))
            );

        /// <summary>
        /// Creates a task.
        /// </summary>
        /// <param name="model">The task to be created.</param>
        /// <response code="201">Indicates that the task was successfully created.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Models.Task))]
        public async Task<IActionResult> PostAsync(Models.Task model) =>
            await ExecuteAsync(async () =>
            {
                var task = await _service.CreateAsync(model);
                return Created($"/{task.Id}", task);
            });

        /// <summary>
        /// Set's a task as "deleted" by it's ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <response code="204">Indicates that the task was successfully deleted.</response>
        /// <response code="404">Indicates that the task with the specified ID was not found.</response>
        [HttpPost("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceExceptionDTO))]
        public async Task<IActionResult> PostIdAsync(Guid id) =>
            await ExecuteAsync(async () =>
            {
                await _service.SetDeletedAtByIdAsync(id);
                return NoContent();
            });

        /// <summary>
        /// Updates a task by it's ID.
        /// </summary>
        /// <param name="id">The ID of the task to be edited.</param>
        /// <param name="model">The task to be edited.</param>
        /// <response code="200">Indicates that the task was successfully updated.</response>
        /// <response code="400">Indicates that the provided IDs are invalid.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceExceptionDTO))]
        public async Task<IActionResult> PutAsync(Guid id, Models.Task model) =>
            await ExecuteAsync(async () => Ok(await _service.UpdateByIdAsync(id, model)));
    }
}
