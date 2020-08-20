using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageDetailText
    {
        public int PageDetailId { get; set; }
        public PageDetail PageDetail { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
