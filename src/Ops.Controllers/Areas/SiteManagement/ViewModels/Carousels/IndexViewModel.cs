using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Carousels
{
    public class IndexViewModel
    {
        public ICollection<Carousel> Carousels { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Carousel Carousel { get; set; }
        public CarouselText CarouselText { get; set; }
    }
}