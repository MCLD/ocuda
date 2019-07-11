using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Locations
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(5)]
        public string BranchCode { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        [MaxLength(255)]
        public string MapLink { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [MaxLength(10)]
        public string Zip { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Facebook { get; set; }

        public int? SubscriptionLinkId { get; set; }

        [MaxLength(6)]
        public string EventLinkId { get; set; }
    }
}
