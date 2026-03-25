using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class SubjectText
    {
        public Language Language { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public Subject Subject { get; set; }

        [Key]
        [Required]
        public int SubjectId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Text { get; set; }
    }
}