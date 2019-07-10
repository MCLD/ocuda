using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Promenade.Models.Entities
{
    class Locations
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        public string BranchCode { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(255)]
        public string MapLink { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(255)]
        public string City { get; set; }

        [MaxLength(255)]
        public string Zip { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public int Facebook { get; set; }

        public int SubscriptionLinkId { get; set; }

        public int EventLinkId { get; set; }
    }
}
