using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class UpdateSetModel
    {
        public int DisplayId { get; set; }
        public string ErrorMessage { get; set; }
        public int SetId { get; set; }
        public IEnumerable<int> SetIds { get; set; }
        public bool Success { get; set; }
    }
}