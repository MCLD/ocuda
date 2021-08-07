using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
