using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Navigation : Abstract.BaseEntity
    {
        public bool ChangeToLinkWhenExtraSmall { get; set; }
        public bool HideTextWhenExtraSmall { get; set; }
        public bool TargetNewWindow { get; set; }
        public IEnumerable<Navigation> Navigations { get; set; }
        public int Order { get; set; }
        [MaxLength(255)]
        public string Icon { get; set; }
        [MaxLength(255)]
        public string Link { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        public int? NavigationTextId { get; set; }

        [NotMapped]
        public NavigationText NavigationText { get; set; }

        public int? NavigationId { get; set; }
    }
}
