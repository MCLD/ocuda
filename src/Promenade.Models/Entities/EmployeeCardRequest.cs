using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmployeeCardRequest : BaseEmployeeCard
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public EmployeeCardDepartment Department { get; set; }
    }
}
