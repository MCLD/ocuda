using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Post : Abstract.BaseEntity
    {
        [NotMapped]
        public List<Category> Categories { get; set; }

        [Required]
        public string Content { get; set; }

        [NotMapped]
        public string CreatedByUsername { get; set; }

        [NotMapped]
        public string PublishedAgo
        {
            get
            {
                return ApproxTimeAgo(PublishedAt);
            }
        }

        public DateTime PublishedAt { get; set; }
        public int SectionId { get; set; }

        [NotMapped]
        public string SectionName { get; set; }

        public bool ShowOnHomePage { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

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