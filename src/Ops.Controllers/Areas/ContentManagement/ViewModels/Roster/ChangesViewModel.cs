﻿using System;
using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class ChangesViewModel
    {
        public ICollection<User> Deactivated { get; set; }

        public string DeactivatedClass
        {
            get
            {
                return Deactivated?.Count > 0 ? "table-warning" : null;
            }
        }

        public TimeSpan Elapsed { get; set; }
        public IDictionary<int, string> Locations { get; set; }
        public ICollection<User> New { get; set; }

        public string NewClass
        {
            get
            {
                return New?.Count > 0 ? "table-success" : null;
            }
        }

        public IDictionary<int, string> NewDivisions { get; set; }
        public IDictionary<int, string> NewLocations { get; set; }
        public IEnumerable<RosterDivision> RemovedDivisions { get; set; }
        public IEnumerable<RosterLocation> RemovedLocations { get; set; }
        public RosterHeader RosterHeader { get; set; }
        public int TotalRows { get; set; }
        public ICollection<User> Verified { get; set; }

        public string VerifiedClass
        {
            get
            {
                return Verified?.Count > 0 ? "table-info" : null;
            }
        }

        public string GetLocation(int locationId)
        {
            return Locations.TryGetValue(locationId, out string value)
                ? value
                : $"Location id {locationId}";
        }
    }
}