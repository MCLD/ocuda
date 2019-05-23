using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class UserMetadataType : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [DisplayName("Is Public")]
        public bool IsPublic { get; set; }
    }
}
