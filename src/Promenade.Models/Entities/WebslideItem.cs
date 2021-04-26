using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class WebslideItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int WebslideId { get; set; }
        public Webslide Webslide { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int Order { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public WebslideItemText WebslideItemText { get; set; }
    }
}
