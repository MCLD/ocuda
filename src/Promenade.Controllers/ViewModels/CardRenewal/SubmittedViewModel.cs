using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.CardRenewal
{
    public class SubmittedViewModel
    {
        public CardRenewalRequest Request { get; set; }
        public SegmentText SegmentText { get; set; }
    }
}
