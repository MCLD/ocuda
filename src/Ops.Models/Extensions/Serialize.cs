using System;
using System.Collections.Generic;
using System.Text;
using BranchLocator.Models;
using BranchLocator.Models.PlaceDetails;
using Newtonsoft.Json;

namespace Ocuda.Ops.Models.Extensions
{
    public static class Serialize
    {
        public static string ToJson(this GeocodePlaceDetails self)
        {
            return JsonConvert.SerializeObject(self, BranchLocator.Models.PlaceDetails.Converter.Settings);
        }

        public static string ToJson(this GeocodePlace self)
        {
            return JsonConvert.SerializeObject(self, BranchLocator.Models.Converter.Settings);
        }
    }
}
