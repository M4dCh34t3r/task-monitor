using System.Net;
using Microsoft.AspNetCore.Mvc;
using TaskMonitor.Utils;

namespace TaskMonitor.Exceptions
{
    public class ContextResultException(HttpStatusCode statusCode, string text = "") : Exception
    {
        public readonly HttpStatusCode StatusCode = statusCode;
        public readonly string Text = text;

        public IActionResult ToObjectResult() =>
            new ObjectResult(new { Text }) { StatusCode = (int)StatusCode };

        public override string ToString() => JsonUtil.Serializar(ToObject());

        private object ToObject() => new { Text };
    }
}
