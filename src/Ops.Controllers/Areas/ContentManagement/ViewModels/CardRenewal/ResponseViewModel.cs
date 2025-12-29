using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.CardRenewal
{
    public class ResponseViewModel
    {
        public SelectList EmailSetups { get; set; }
        public EmailSetupText EmailSetupText { get; set; }
        public ICollection<Language> Languages { get; set; }
        public CardRenewalResponse Response { get; set; }
    }
}
