using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Promenade.Models.Entities
{
    public class Location
    {
        [Key]
        [Required]
        public int Id { get; set; }

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

        [NotMapped]
        public SegmentText DescriptionSegment { get; set; }

        [MaxLength(255)]
        public string MapLink { get; set; }

        [MaxLength(100)]
        public string MapImagePath { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [MaxLength(10)]
        public string Zip { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        public int? DisplayGroupId { get; set; }

        public Group DisplayGroup { get; set; }

        [DisplayName("Description Segment")]
        public int DescriptionSegmentId { get; set; }

        [DisplayName("Post-Feature Segment")]
        public int? PostFeatureSegmentId { get; set; }

        [DisplayName("Pre-Feature Segment")]
        public int? PreFeatureSegmentId { get; set; }

        [MaxLength(50)]
        public string Facebook { get; set; }

        [MaxLength(100)]
        public string SubscriptionLink { get; set; }

        [MaxLength(255)]
        public string EventLink { get; set; }

        public bool HasEvents { get; set; }

        [MaxLength(50)]
        public string AdministrativeArea { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(50)]
        public string Type { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string AreaServedName { get; set; }

        [MaxLength(50)]
        public string AreaServedType { get; set; }

        [MaxLength(50)]
        public string AddressType { get; set; }

        [MaxLength(50)]
        public string ContactType { get; set; }

        [MaxLength(50)]
        public string ParentOrganization { get; set; }

        public bool IsAccessibleForFree { get; set; }

        [MaxLength(255)]
        public string GeoLocation { get; set; }

        [MaxLength(50)]
        public string PAbbreviation { get; set; }

        [MaxLength(50)]
        public string PriceRange { get; set; }

        public bool IsAlwaysOpen { get; set; }

        [MaxLength(50)]
        public string LocatorName { get; set; }

        [MaxLength(50)]
        public string LocatorNotes { get; set; }

        [NotMapped]
        public double Distance { get; set; }

        [NotMapped]
        public bool Open { get; set; }

        [NotMapped]
        public List<Location> CloseLocations { get; set; }

        [NotMapped]
        public List<LocationDayGrouping> LocationHours { get; set; }

        [NotMapped]
        public LocationHoursResult CurrentStatus { get; set; }

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

        public bool IsClosed { get; set; }

        public int? HourOverrideSegmentId { get; set; }
    }
}
