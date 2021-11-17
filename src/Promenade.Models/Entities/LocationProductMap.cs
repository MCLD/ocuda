using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationProductMap
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string ImportLocation { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}