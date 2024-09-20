using Microsoft.AspNetCore.Mvc;

namespace RequestLoggingMiddlewareLib.Models
{
    public class CustomProblemDetails : ProblemDetails
    {
        public IDictionary<string, object?>? CustomExtensions { get; set; }
    }
}
