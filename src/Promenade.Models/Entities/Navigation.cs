using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Navigation
    {
        [DisplayName("Change to Link When XS")]
        public bool ChangeToLinkWhenExtraSmall { get; set; }

        [DisplayName("Hide Text When XS")]
        public bool HideTextWhenExtraSmall { get; set; }

        [MaxLength(255)]
        public string Icon { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int? NavigationId { get; set; }

        [NotMapped]
        public ICollection<string> NavigationLanguages { get; set; }

        public IEnumerable<Navigation> Navigations { get; set; }

        [NotMapped]
        public NavigationText NavigationText { get; set; }

        public int Order { get; set; }

        [NotMapped]
        public int SubnavigationCount { get; set; }

        [DisplayName("Target New Window")]
        public bool TargetNewWindow { get; set; }
    }
}
