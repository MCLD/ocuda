using System.ComponentModel;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SocialCards
{
    public class DetailViewModel
    {
        public string Action { get; set; }

        [DisplayName("Associated with location")]
        public string LocationStub { get; set; }

        public SocialCard SocialCard { get; set; }
    }
}