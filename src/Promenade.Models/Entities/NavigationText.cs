using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavigationText
    {
        public int NavigationId { get; set; }
        public Navigation Navigation { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [MaxLength(255)]
        public string Label { get; set; }

        [MaxLength(255)]
        public string Link { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }
    }
}
