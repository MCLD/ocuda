using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.CardRenewal
{
    public class ResponsesViewModel
    {
        public IEnumerable<CardRenewalResponse> Responses { get; set; }
        public CardRenewalResponse Response { get; set; }
    }
}
