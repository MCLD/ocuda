using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;

namespace Ocuda.Promenade.Controllers.ViewModels.CardRenewal
{
    public class IndexViewModel
    {
        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [DisplayName(i18n.Keys.Promenade.PromptLibraryCardNumber)]
        public string Barcode { get; set; }

        public bool Invalid { get; set; }

        public string ForgotPasswordLink { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [DisplayName(i18n.Keys.Promenade.PromptPassword)]
        public string Password { get; set; }

        public SegmentText SegmentText { get; set; }
    }
}
