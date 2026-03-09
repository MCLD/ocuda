using System.ComponentModel.DataAnnotations;
using Ocuda.Models;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmployeeCardRequest : BaseEmployeeCard
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public EmployeeCardDepartment Department { get; set; }

        // Adds foreign key constraint to LanguageId for Promenade
        public Language Language { get; set; }
    }
}
