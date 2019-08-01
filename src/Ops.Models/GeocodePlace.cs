using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BranchLocator.Models
{
    public partial class GeocodePlace
    {
        [JsonProperty("html_attributions")]
        public string[] HtmlAtrributions { get; set; }

        [JsonProperty("results")]
        public Result[] Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public partial class Result
    {
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("opening_hours")]
        public OpeningHours OpeningHours { get; set; }

        [JsonProperty("photos")]
        public Photo[] Photos { get; set; }

        [JsonProperty("plus_code")]
        public PlusCode PlusCode { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("price_level")]
        public int PriceLevel { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }

        [JsonProperty("aspect")]
        public Aspect[] Aspect { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("user_ratings_total")]
        public int UserRatingsTotal { get; set; }

    }
    public partial class Geometry
    {
        [JsonProperty("location")]
        public Locate Location { get; set; }

    }
    public partial class PlusCode
    {
        [JsonProperty("compound_code")]
        public string CompoundCode { get; set; }

        [JsonProperty("global_code")]
        public string GlobalCode { get; set; }
    }

    public partial class Locate
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }

    }
    public partial class Photo
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("html_attributions")]
        public string[] HtmlAttributions { get; set; }

        [JsonProperty("photo_reference")]
        public string PhotoReference { get; set; }
    }

    public partial class OpeningHours
    {
        [JsonProperty("open_now")]
        public bool OpenNow { get; set; }

    }

    public partial class Aspect
    {
        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }

    public partial class GeocodeRoutes
    {
        public static GeocodeRoutes FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GeocodeRoutes>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this GeocodeRoutes self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
    }

    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}

