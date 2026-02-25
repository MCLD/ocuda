using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Topic
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
    }
}