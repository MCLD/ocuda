using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class DetailsViewModel
    {
        public bool IsNew { get; set; }
        public bool IsProductManger { get; set; }
        public Product Product { get; set; }
        public string SegmentName { get; set; }
    }
}
