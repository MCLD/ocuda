using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages
{
    public class DetailViewModel
    {
        public Page Page { get; set; }
        public bool Publish { get; set; }
        public string Action { get; set; }
    }
}
