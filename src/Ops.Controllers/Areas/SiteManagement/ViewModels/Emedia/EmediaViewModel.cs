using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia
{
    public class EmediaViewModel
    {
        public PaginateModel PaginateModel { get; set; }

        public ICollection<Promenade.Models.Entities.Emedia> AllEmedia { get; set; }

        public Promenade.Models.Entities.Emedia Emedia { get; set; }

        public SelectList SelectionEmediaCategories { get; set; }

        [DisplayName("Emedia Categories")]
        public List<int> CategoryIds { get; set; }
    }
}
