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
    public class ProjectController(IProjectService service) : AjaxController
    {
        private readonly IProjectService _service = service;

        /// <summary>
        /// Read all projects.
        /// </summary>
        /// <response code="200">Indicates that the projects were successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Project[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ContextResultDTO))]
        [ProducesResponseType(
            StatusCodes.Status500InternalServerError,
            Type = typeof(StatusCodeResult)
        )]
        public async Task<IActionResult> GetAsync(bool includeTasks) =>
            await ExecuteAsync(async () => Ok(await _service.ReadAllAsync(includeTasks)));

        /// <summary>
        /// Creates a project.
        /// </summary>
        /// <param name="model">The project to be created.</param>
        /// <response code="201">Indicates that the project was successfully created.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Models.Project))]
        public async Task<IActionResult> PostAsync(Models.Project model) =>
            await ExecuteAsync(async () =>
            {
                var project = await _service.CreateAsync(model);
                return Created($"/{project.Id}", project);
            });

        /// <summary>
        /// Set's a project as "deleted" by it's ID.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <response code="204">Indicates that the project was successfully deleted.</response>
        /// <response code="404">Indicates that the project with the specified ID was not found.</response>
        [HttpPost("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceExceptionDTO))]
        public async Task<IActionResult> PostIdAsync(Guid id) =>
            await ExecuteAsync(async () =>
            {
                await _service.SetDeletedAtByIdAsync(id);
                return NoContent();
            });

        /// <summary>
        /// Updates a project by it's ID.
        /// </summary>
        /// <param name="id">The ID of the project to be edited.</param>
        /// <param name="model">The project to be edited.</param>
        /// <response code="200">Indicates that the project was successfully updated.</response>
        /// <response code="400">Indicates that the provided IDs are invalid.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Project))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceExceptionDTO))]
        public async Task<IActionResult> PutAsync(Guid id, Models.Project model) =>
            await ExecuteAsync(async () => Ok(await _service.UpdateByIdAsync(id, model)));
    }
}
