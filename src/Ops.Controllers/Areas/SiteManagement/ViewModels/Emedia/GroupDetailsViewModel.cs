using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia
{
    public class GroupDetailsViewModel
    {
        public EmediaGroup EmediaGroup { get; set; }
        public ICollection<Promenade.Models.Entities.Emedia> Emedias { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Promenade.Models.Entities.Emedia Emedia { get; set; }
    }
}
