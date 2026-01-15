using static Ocuda.Ops.Models.Entities.CardRenewalResponse;

namespace Ocuda.Ops.Service.Models.CardRenewal
{
    public class ProcessResult
    {
        public bool EmailNotUpdated { get; set; }
        public bool EmailSent { get; set; }
        public ResponseType Type { get; set; }
    }
}
