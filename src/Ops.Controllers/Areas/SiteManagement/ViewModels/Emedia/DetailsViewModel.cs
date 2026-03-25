using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia
{
    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            CategoryList = [];
            CategorySelection = [];
            SubjectList = [];
            SubjectSelection = [];
        }

        public Promenade.Models.Entities.Emedia Emedia { get; set; }
        public EmediaText EmediaText { get; set; }

        public ICollection<Subject> SubjectList { get; }
        public ICollection<int> SubjectSelection { get; }
        public ICollection<Category> CategoryList { get; }
        public ICollection<int> CategorySelection { get; }
        public string CategorySelectionText { get; set; }
        public string SubjectSelectionText { get; set; }

        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}
