using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BranchLocator.Models
{
    public partial class GeocodeResult
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }
    }

    public partial class AddressComponent
    {
        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }
    }

    public partial class Geometry
    {
        [JsonProperty("location")]
        public Locate Location { get; set; }

        [JsonProperty("bounds")]
        public Bounds Bounds { get; set; }

        [JsonProperty("location_type")]
        public string LocationType { get; set; }

        [JsonProperty("viewport")]
        public Bounds Viewport { get; set; }
    }

    public partial class Locate
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public partial class Bounds
    {
        [JsonProperty("northeast")]
        public Locate Northeast { get; set; }

        [JsonProperty("southwest")]
        public Locate Southwest { get; set; }
    }


    public partial class GeocodeResult
    {
        public static GeocodeResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GeocodeResult>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this GeocodeResult self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}

