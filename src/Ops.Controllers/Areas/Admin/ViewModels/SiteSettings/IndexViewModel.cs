using System.Collections.Generic;
using System.ComponentModel;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.SiteSettings
{
    public class IndexViewModel
    {
        public SiteSetting SiteSetting { get; set; }
        public Dictionary<string, List<SiteSetting>> SiteSettingsByCategory { get; set; }
        public string ValueBool { get; set; }
        public string ValueInt { get; set; }
        public string ValueString { get; set; }
    }
}
