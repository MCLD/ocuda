using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Controllers.ViewModels.Shared;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public IEnumerable<SectionPost> SectionPosts { get; set; }
        public IEnumerable<SectionLink> SectionLinks { get; set; }
        public IEnumerable<SectionFile> SectionFiles { get; set; }
        public IEnumerable<SectionCalendar> SectionCalendars { get; set; }
    }
}
