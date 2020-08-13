using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Controllers.ViewModels.Podcasts
{
    public class IndexViewModel
    {
        public ICollection<Podcast> Podcasts { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
