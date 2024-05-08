using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature
    {
        [NotMapped]
        public string BodyText { get; set; }

        [NotMapped]
        public string DisplayName { get; set; }

        [MaxLength(48)]
        [Required]
        public string Icon { get; set; }

        [MaxLength(5)]
        [NotMapped]
        public string IconText { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [Required]
        public bool IsAtThisLocation { get; set; }

        [NotMapped]
        public bool IsNewFeature { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int NameSegmentId { get; set; }

        [NotMapped]
        public bool NeedsPopup { get; set; }

        public int? SortOrder { get; set; }

        [Display(Name = "Slug")]
        [MaxLength(80)]
        public string Stub { get; set; }

        public int? TextSegmentId { get; set; }
    }
}