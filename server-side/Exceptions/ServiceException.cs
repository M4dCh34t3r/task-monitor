using System.Net;
using Microsoft.AspNetCore.Mvc;
using TaskMonitor.DTOs;
using TaskMonitor.Enums;
using TaskMonitor.Utils;

namespace TaskMonitor.Exceptions
{
    public class ServiceException(
        HttpStatusCode statusCode,
        ModalTheme theme = ModalTheme.Ignore,
        string title = "",
        string text = ""
    ) : Exception
    {
        public static readonly ServiceException InvalidRequestData =
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Error,
                "Invalid request data",
                "The request content is not as expected"
            );

        public readonly HttpStatusCode StatusCode = statusCode;
        public readonly ModalTheme Theme = theme;
        public readonly string Title = title;
        public readonly string Text = text;

        public IActionResult ToObjectResult() =>
            new ObjectResult(ToObject()) { StatusCode = (int)StatusCode };

        public override string ToString() => JsonUtil.Serializar(ToObject());

        private ServiceExceptionDTO ToObject() =>
            new()
            {
                Theme = Theme,
                Title = Title,
                Text = Text,
            };
    }
}
