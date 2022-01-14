using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations
{
    public class DetailsViewModel
    {
        public Navigation Navigation { get; set; }
        public RoleProperties RoleProperties { get; set; }
        public ICollection<Navigation> Navigations { get; set; }

        public int NavigationId { get; set; }
        public NavigationText NavigationText { get; set; }
        public bool NewNavigationText { get; set; }

        public string LanguageDescription { get; set; }
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}
