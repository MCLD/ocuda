using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.RenewCard
{
    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            CustomerBlocks = [];
            MaxNotesDisplayLength = 400;
        }

        public string AcceptedCounty { get; set; }

        public bool AddressLookupConfigured { get; set; }

        public Customer Customer { get; set; }

        public int? CustomerAge { get; set; }

        public ICollection<CustomerBlock> CustomerBlocks { get; }

        public string CustomerBlocksErrorMessage { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerCodeErrorMessage { get; set; }

        public string CustomerName { get; set; }

        public bool InCounty { get; set; }

        public string LeapPath { get; set; }

        public int MaxNotesDisplayLength { get; }

        public bool OverChargesLimit { get; set; }
        public RenewCardRequest Request { get; set; }
        public int RequestId { get; set; }

        [DisplayName("Response")]
        [Required]
        public int? ResponseId { get; set; }

        public IEnumerable<SelectListItem> ResponseList { get; set; }

        [DisplayName("Email Text")]
        [Required]
        public string ResponseText { get; set; }

        public RenewCardResult Result { get; set; }
    }
}