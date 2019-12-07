using System.Collections.Generic;
using System.ComponentModel;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SiteSettings
{
    public class IndexViewModel
    {
        public SiteSetting SiteSetting { get; set; }
        public Dictionary<string, List<SiteSetting>> SiteSettingsByCategory { get; set; }
    }
}
