using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.RenewCard
{
    public class RenewCardResponsesViewModel
    {
        public RenewCardResponse Response { get; set; }
        public IEnumerable<RenewCardResponse> Responses { get; set; }
    }
}