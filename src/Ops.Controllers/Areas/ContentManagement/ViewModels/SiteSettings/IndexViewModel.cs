using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.SiteSettings
{
    public class IndexViewModel
    {
        public SiteSetting SiteSetting { get; set; }
        public Dictionary<string, List<SiteSetting>> SiteSettingsByCategory { get; set; }
    }
}