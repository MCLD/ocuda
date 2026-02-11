using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeSignup
{
    public class PendingViewModel
    {
        public EmployeeCardRequest CardRequest { get; set; }
        public EmployeeCardNote Note { get; set; }
        public bool APIConfigured { get; set; }

        [DisplayName("Card Number")]
        [MaxLength(16)]
        public string CardNumber { get; set; }

        public int RequestId { get; set; }
        public EmployeeCardResult.ResultType? Type { get; set; }
    }
}