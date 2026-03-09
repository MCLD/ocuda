using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.RenewCard
{
    public class RenewCardResponseViewModel
    {
        public SelectList EmailSetups { get; set; }
        public EmailSetupText EmailSetupText { get; set; }
        public IEnumerable<Language> Languages { get; set; }
        public RenewCardResponse Response { get; set; }
    }
}