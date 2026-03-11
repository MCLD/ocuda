using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class EmployeeCardNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int EmployeeCardRequestId { get; set; }

        [DisplayName("Staff Note")]
        [MaxLength(2000)]
        public string StaffNote { get; set; }
    }
}
