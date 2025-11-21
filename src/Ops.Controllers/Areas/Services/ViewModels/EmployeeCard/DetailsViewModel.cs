using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeCard
{
    public class DetailsViewModel
    {
        public EmployeeCardRequest CardRequest { get; set; }
        public SubmitType Action {get; set; }

        public enum SubmitType
        {
            SaveNotes,
            ProcessNoEmail,
            ProcessWithEmail,
            CreateCard
        }
    }
}
