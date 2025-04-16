using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SocialCards
{
    public class IndexViewModel
    {
        public ICollection<SocialCard> SocialCards { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public SocialCard SocialCard { get; set; }
    }
}