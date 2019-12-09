using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Location : BaseEntity
    {
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(5)]
        public string Code { get; set; }

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

        [MaxLength(15)]
        public string Phone { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Facebook { get; set; }

        [MaxLength(255)]
        public string SubscriptionLink { get; set; }

        [MaxLength(255)]
        public string EventLink { get; set; }

        public string AdministrativeArea { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        [MaxLength(255)]
        public string GeoLocation { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        [NotMapped]
        public double Distance { get; set; }

        public bool Open { get; set; }

        public bool ShowLocation { get; set; }

        public string PAbbreviation { get; set; }

        public bool IsAlwaysOpen { get; set; }

        [NotMapped]
        public List<Location> CloseLocations { get; set; }

        [NotMapped]
        public List<string> LocationHours { get; set; }

        [NotMapped]
        public string FormattedAddress { get; set; }

        [NotMapped]
        public SelectList EventDistanceOptions { get; set; }

        [NotMapped]
        public List<int> DefaultLibIds { get; set; }

        [NotMapped]
        public List<string> WeeklyHours { get; set; }

        [NotMapped]
        public bool IsNewLocation { get; set; }

        public int? SocialCardId { get; set; }

        public SocialCard SocialCard { get; set; }
    }
}
