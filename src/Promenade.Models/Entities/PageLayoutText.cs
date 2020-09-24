using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageLayoutText
    {
        public int PageLayoutId { get; set; }
        public PageLayout PageLayout { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
