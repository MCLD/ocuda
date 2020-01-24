using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CategoryText
    {
        [Key]
        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [MaxLength(255)]
        [Required]
        public string Text { get; set; }
    }
}
