using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Clc.Polaris.Api.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.CardRenewal
{
    public class DetailsViewModel
    {
        private const int _maxNotesDisplayLength = 400;

        public CardRenewalRequest Request { get; set; }
        public CardRenewalResult Result { get; set; }
        public PatronData PatronData { get; set; }
        public string AcceptedCounty { get; set; }
        public string AddressLookupPath { get; set; }
        public string AssessorLookupPath { get; set; }
        public bool InCounty { get; set; }
        public bool IsJuvenile { get; set; }
        public string LeapPath { get; set; }
        public int? PatronAge { get; set; }
        public string PatronCode { get; set; }
        public string PatronName { get; set; }
        public IEnumerable<SelectListItem> ResponseList { get; set; }

        public int RequestId { get; set; }

        [DisplayName("Response")]
        [Required]
        public int? ResponseId { get; set; }

        [DisplayName("Email Text")]
        [Required]
        public string ResponseText { get; set; }

        public int MaxNotesDisplayLength
        {
            get { return _maxNotesDisplayLength; }
        }
    }
}
