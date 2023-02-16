using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.UserSync
{
    public class ChangeReportViewModel
    {
        public bool AllowPerformSync { get; set; }
        public bool AllowUpdateLocations { get; set; }
        public bool IsApplied { get; set; }
        public StatusReport Status { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }

        public static string GetClass(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Critical => "table-danger",
            LogLevel.Error => "table-danger",
            LogLevel.Information => "table-success",
            LogLevel.Warning => "table-warning",
            _ => null
        };
    }
}