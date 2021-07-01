using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageLayoutText
    {
        [DisplayName("Is the title only visible to screen readers?")]
        public bool IsTitleHidden { get; set; }

        public Language Language { get; set; }
        public int LanguageId { get; set; }
        public PageLayout PageLayout { get; set; }
        public int PageLayoutId { get; set; }

        [Required]
        [MaxLength(255)]
        [DisplayName("Page title")]
        public string Title { get; set; }
    }z
}