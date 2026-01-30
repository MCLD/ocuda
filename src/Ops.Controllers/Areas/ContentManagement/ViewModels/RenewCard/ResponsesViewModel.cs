using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.RenewCard
{
    public class ResponsesViewModel
    {
        public IEnumerable<RenewCardResponse> Responses { get; set; }
        public RenewCardResponse Response { get; set; }
    }
}
