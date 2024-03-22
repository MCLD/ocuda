using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationInteriorImage
    {
        [Key]
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string ImagePath { get; set; }
        public int SortOrder { get; set; }

        [NotMapped]
        public ImageAltText AltText { get; set; }
    }
}
