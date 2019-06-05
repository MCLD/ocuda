using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages
{
    public class DetailViewModel
    {
        public Page Page { get; set; }
        public bool Publish { get; set; }
        public string Action { get; set; }
        public int SectionId { get; set; }
        public IEnumerable<File> Attachments { get; set; }
    }
}
