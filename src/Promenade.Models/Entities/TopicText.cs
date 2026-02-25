using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class TopicText
    {
        public Language Language { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Text { get; set; }

        public Topic Topic { get; set; }

        [Key]
        [Required]
        public int TopicId { get; set; }
    }
}