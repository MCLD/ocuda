using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Models
{
    public class StatusMessage
    {
        public string Message { get; set; }
        public LogLevel Status { get; set; }
    }
}