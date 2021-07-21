using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Navigation
    {
        public bool ChangeToLinkWhenExtraSmall { get; set; }

        public bool HideTextWhenExtraSmall { get; set; }

        [MaxLength(255)]
        public string Icon { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int? NavigationId { get; set; }
        public IEnumerable<Navigation> Navigations { get; set; }

        [NotMapped]
        public NavigationText NavigationText { get; set; }

        public int? NavigationTextId { get; set; }
        public int Order { get; set; }
        public bool TargetNewWindow { get; set; }
    }
}