using static Ocuda.Ops.Models.Entities.RenewCardResponse;

namespace Ocuda.Ops.Service.Models.RenewCard
{
    public class ProcessResult
    {
        public bool EmailNotUpdated { get; set; }
        public bool EmailSent { get; set; }
        public ResponseType Type { get; set; }
    }
}
