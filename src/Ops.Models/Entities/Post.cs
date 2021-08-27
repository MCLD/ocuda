using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Models.Entities
{
    public class Post : Abstract.BaseEntity
    {
        [NotMapped]
        public string BorderColor
        {
            get
            {
                if (!PublishedAt.HasValue)
                {
                    return "border-warning";
                }
                return IsPinned ? "border-info" : null;
            }
        }

        [Required]
        public string Content { get; set; }

        [NotMapped]
        public string CreatedByUsername { get; set; }

        [NotMapped]
        public bool IsPinned
        {
            get
            {
                return PinnedUntil.HasValue && DateTime.Now <= PinnedUntil.Value;
            }
        }

        [Display(Name = "Pinned until")]
        public DateTime? PinnedUntil { get; set; }

        [NotMapped]
        public string PublishedAgo
        {
            get
            {
                return PublishedAt?.ApproxTimeAgo();
            }
        }

        [NotMapped]
        public string UpdatedAgo
        {
            get
            {
                return UpdatedAt?.ApproxTimeAgo();
            }
        }

        [Display(Name = "Published At")]
        public DateTime? PublishedAt { get; set; }

        public Section Section { get; set; }

        public int SectionId { get; set; }

        [NotMapped]
        public string SectionName { get; set; }

        [NotMapped]
        public string SectionSlug { get; set; }

        [Display(Name = "Featured")]
        public bool ShowOnHomePage { get; set; }

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; }

        [NotMapped]
        public string TextColor
        {
            get
            {
                return IsPinned ? "text-info" : null;
            }
        }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [NotMapped]
        public string UpdatedByUsername { get; set; }
    }
}