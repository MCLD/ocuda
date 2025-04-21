using System.Collections.Generic;
using System.ComponentModel;
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

        public int EmediaGroupId { get; set; }
        public Promenade.Models.Entities.Segment Segment { get; set; }

        [DisplayName("Languages")]
        public ICollection<string> SegmentLanguages { get; set; }
    }
}
