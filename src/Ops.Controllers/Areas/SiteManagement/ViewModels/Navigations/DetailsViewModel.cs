using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations
{
    public class DetailsViewModel
    {
        public bool CanDeleteText { get; set; }
        public string LanguageDescription { get; set; }
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }
        public Navigation Navigation { get; set; }
        public ICollection<Navigation> Navigations { get; set; }
        public NavigationText NavigationText { get; set; }
        public RoleProperties RoleProperties { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}
