using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.RenewCard
{
    public class ResponseViewModel
    {
        public SelectList EmailSetups { get; set; }
        public EmailSetupText EmailSetupText { get; set; }
        public ICollection<Language> Languages { get; set; }
        public RenewCardResponse Response { get; set; }
    }
}
