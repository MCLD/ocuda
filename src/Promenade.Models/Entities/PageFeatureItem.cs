using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageFeatureItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PageFeatureId { get; set; }
        public PageFeature PageFeature { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int Order { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public PageFeatureItemText PageFeatureItemText { get; set; }
    }
}
