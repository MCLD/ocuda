using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class TitleClassMapping
    {
        [Key]
        [Required]
        public int TitleClassId { get; set; }

        [Key]
        [Required]
        [MaxLength(255)]
        public string UserTitle { get; set; }
    }
}