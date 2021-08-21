using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia
{
    public class DetailsViewModel
    {
        public Promenade.Models.Entities.Emedia Emedia { get; set; }
        public EmediaText EmediaText { get; set; }

        public ICollection<Promenade.Models.Entities.Category> CategoryList { get; set; }
        public ICollection<int> CategorySelection { get; set; }

        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}
