using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Post : Abstract.BaseEntity
    {
        [NotMapped]
        public string BorderColor
        {
            get
            {
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

        public DateTime? PinnedUntil { get; set; }

        [NotMapped]
        public string PublishedAgo
        {
            get
            {
                return ApproxTimeAgo(PublishedAt);
            }
        }

        [Display(Name = "Published At")]
        public DateTime PublishedAt { get; set; }

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

        private static string ApproxTimeAgo(DateTime date)
        {
            if (date == default) { return null; }
            var diff = DateTime.Now - date;
            if (diff.TotalDays > 1)
            {
                var days = Math.Floor(diff.TotalDays);
                return $"{days:n0} {(days == 1 ? "day" : "days")} ago";
            }
            if (diff.TotalHours > 1)
            {
                var hours = Math.Floor(diff.TotalHours);
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }

            return "recently";
        }
    }
}