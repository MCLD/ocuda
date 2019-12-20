using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Category
{
    public class CategoryViewModel
    {
        public PaginateModel PaginateModel { get; set; }

        public ICollection<Promenade.Models.Entities.Category> AllCategories { get; set; }

        public Promenade.Models.Entities.Category Category { get; set; }
    }
}
