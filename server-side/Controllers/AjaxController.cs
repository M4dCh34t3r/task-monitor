using Microsoft.AspNetCore.Mvc;
using Serilog;
using TaskMonitor.Exceptions;

namespace TaskMonitor.Controllers
{
    [ApiController]
    [Route("/Ajax/[controller]")]
    public class AjaxController() : ControllerBase
    {
        protected IActionResult Execute(Func<IActionResult> func)
        {
            try
            {
                return func();
            }
            catch (ServiceException ex)
            {
                Log.Debug(ex, "Expected \"service exception\" occurred when processing request");
                return ex.ToObjectResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected \"exception\" occurred when processing request");
                return StatusCode(500);
            }
        }

        protected async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> func)
        {
            try
            {
                return await func();
            }
            catch (ServiceException ex)
            {
                Log.Debug(ex, "Expected \"service exception\" occurred when processing request");
                return ex.ToObjectResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected \"exception\" occurred when processing request");
                return StatusCode(500);
            }
        }
    }
}
