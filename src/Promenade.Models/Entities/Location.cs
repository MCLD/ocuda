﻿using System.Collections.Generic;
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

        [MaxLength(255)]
        public string MapLink { get; set; }

        [MaxLength(255)]
        public string MapImagePath { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        [MaxLength(10)]
        public string Zip { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        public int? DisplayGroupId { get; set; }

        public Group DisplayGroup { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [MaxLength(2000)]
        public string PostFeatureDescription { get; set; }

        [MaxLength(255)]
        public string Facebook { get; set; }

        [MaxLength(255)]
        public string SubscriptionLink { get; set; }

        [MaxLength(255)]
        public string EventLink { get; set; }

        public bool HasEvents { get; set; }

        [MaxLength(255)]
        public string AdministrativeArea { get; set; }

        [MaxLength(255)]
        public string Country { get; set; }

        [MaxLength(255)]
        public string State { get; set; }

        [MaxLength(255)]
        public string SDType { get; set; }

        [MaxLength(255)]
        public string SDEmail { get; set; }

        [MaxLength(255)]
        public string SDAreaServedName { get; set; }

        [MaxLength(255)]
        public string SDAreaServedType { get; set; }

        [MaxLength(255)]
        public string SDLocationId { get; set; }

        [MaxLength(255)]
        public string SDParentOrganization { get; set; }

        [MaxLength(255)]
        public string GeoLocation { get; set; }

        [MaxLength(255)]
        public string PAbbreviation { get; set; }

        public bool IsAlwaysOpen { get; set; }

        [MaxLength(255)]
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
    }
}
