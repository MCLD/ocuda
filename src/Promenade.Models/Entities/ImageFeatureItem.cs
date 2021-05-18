using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageFeatureItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int ImageFeatureId { get; set; }
        public ImageFeature ImageFeature { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int Order { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public ImageFeatureItemText ImageFeatureItemText { get; set; }
    }
}