using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Promenade.Models.Entities
{
    public class Location
    {
        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string AddressType { get; set; }

        [MaxLength(50)]
        public string AdministrativeArea { get; set; }

        [MaxLength(50)]
        public string AreaServedName { get; set; }

        [MaxLength(50)]
        public string AreaServedType { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [NotMapped]
        public List<Location> CloseLocations { get; set; }

        [MaxLength(5)]
        [DisplayName("Short code")]
        public string Code { get; set; }

        [MaxLength(50)]
        public string ContactType { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }

        [NotMapped]
        public LocationHoursResult CurrentStatus { get; set; }

        [NotMapped]
        public List<int> DefaultLibIds { get; set; }

        [NotMapped]
        public SegmentText DescriptionSegment { get; set; }

        [DisplayName("Description Segment")]
        public int DescriptionSegmentId { get; set; }

        public Group DisplayGroup { get; set; }

        public int? DisplayGroupId { get; set; }

        [NotMapped]
        public double Distance { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [NotMapped]
        public SelectList EventDistanceOptions { get; set; }

        [MaxLength(255)]
        [DisplayName("Link to events")]
        public string EventLink { get; set; }

        [MaxLength(50)]
        [DisplayName("Link to Facebook page")]
        public string Facebook { get; set; }

        [NotMapped]
        public string FormattedAddress { get; set; }

        [MaxLength(255)]
        public string GeoLocation { get; set; }

        public bool HasEvents { get; set; }

        [DisplayName("Hours Override Segment")]
        public int? HoursSegmentId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        public bool IsAccessibleForFree { get; set; }

        public bool IsAlwaysOpen { get; set; }

        [DisplayName("Mark as closed, override hours")]
        public bool IsClosed { get; set; }

        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool IsNewLocation { get; set; }

        [NotMapped]
        public List<LocationDayGrouping> LocationHours { get; set; }

        [MaxLength(50)]
        public string LocatorName { get; set; }

        [MaxLength(50)]
        public string LocatorNotes { get; set; }

        [MaxLength(100)]
        public string MapImagePath { get; set; }

        [MaxLength(255)]
        [DisplayName("Link to map")]
        public string MapLink { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [NotMapped]
        public bool Open { get; set; }

        [MaxLength(50)]
        public string PAbbreviation { get; set; }

        [MaxLength(50)]
        public string ParentOrganization { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [DisplayName("Post-Feature Segment")]
        public int? PostFeatureSegmentId { get; set; }

        [DisplayName("Pre-Feature Segment")]
        public int? PreFeatureSegmentId { get; set; }

        [MaxLength(50)]
        public string PriceRange { get; set; }

        public SocialCard SocialCard { get; set; }

        [DisplayName("Social card")]
        public int? SocialCardId { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        [MaxLength(100)]
        [DisplayName("Email subscription link")]
        public string SubscriptionLink { get; set; }

        [MaxLength(50)]
        public string Type { get; set; }

        [NotMapped]
        public List<string> WeeklyHours { get; set; }

        [MaxLength(10)]
        public string Zip { get; set; }
    }
}