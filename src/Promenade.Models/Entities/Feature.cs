using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature
    {
        [NotMapped]
        [Display(Name = "Text")]
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
        [Display(Name = "Section")]
        public bool IsAtThisLocation { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int NameSegmentId { get; set; }

        [Display(Name = "Sort order")]
        public int? SortOrder { get; set; }

        [Display(Name = "Slug")]
        [MaxLength(80)]
        public string Stub { get; set; }

        public int? TextSegmentId { get; set; }
    }
}