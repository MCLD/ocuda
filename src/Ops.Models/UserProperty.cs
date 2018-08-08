using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class UserProperty : Abstract.BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int UserPropertyTypeId { get; set; }
        public UserPropertyType UserPropertyType { get; set; }

        [Required]
        [MaxLength(255)]
        public string Value { get; set; }
    }
}
