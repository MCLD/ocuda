using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Categories
{
    public class DetailsViewModel
    {
        public Category Category { get; set; }
        public CategoryText CategoryText { get; set; }

        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}
