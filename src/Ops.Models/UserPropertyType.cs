using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class UserPropertyType : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Column(TypeName = "int")]
        public UserPropertyTypeType Type { get; set; }

        public bool CanHaveMultiple { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUserSettable { get; set; }

        public enum UserPropertyTypeType
        {
            Bool,
            Int,
            String
        }
    }
}
