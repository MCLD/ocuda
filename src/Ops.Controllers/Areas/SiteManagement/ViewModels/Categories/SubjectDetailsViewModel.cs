using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Categories
{
    public class SubjectDetailsViewModel
    {
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public Subject Subject { get; set; }
        public SubjectText SubjectText { get; set; }
    }
}