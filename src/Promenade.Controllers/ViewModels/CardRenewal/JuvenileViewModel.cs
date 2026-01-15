using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;

namespace Ocuda.Promenade.Controllers.ViewModels.CardRenewal
{
    public class JuvenileViewModel
    {
        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [DisplayName(i18n.Keys.Promenade.PromptGuardianBarcode)]
        public string GuardianBarcode { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [DisplayName(i18n.Keys.Promenade.PromptGuardianName)]
        public string GuardianName { get; set; }

        public SegmentText SegmentText { get; set; }
    }
}
