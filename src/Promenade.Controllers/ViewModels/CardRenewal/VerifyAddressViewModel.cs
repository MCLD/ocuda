using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;

namespace Ocuda.Promenade.Controllers.ViewModels.CardRenewal
{
    public class VerifyAddressViewModel
    {
        public IEnumerable<CustomerAddress> Addresses { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [DisplayName(i18n.Keys.Promenade.PromptEmail)]
        public string Email { get; set; }

        public SegmentText HeaderSegmentText { get; set; }
        public SegmentText NoAddressSegmentText {get; set; }
        public bool SameAddress { get; set; }
    }
}
