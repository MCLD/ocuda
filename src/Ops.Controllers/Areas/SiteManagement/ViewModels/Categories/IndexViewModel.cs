using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Categories
{
    public class IndexViewModel
    {
        public ICollection<Category> Categories { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Category Category { get; set; }
    }
}