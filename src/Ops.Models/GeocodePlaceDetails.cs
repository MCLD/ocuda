using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BranchLocator.Models.PlaceDetails
{
    public partial class GeocodePlaceDetails
    {
        [JsonProperty("html_attributions")]
        public string[] HtmlAtrributions { get; set; }

        [JsonProperty("result")]
        public PlaceDetailsResult Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class PlaceDetailsResult
    {
        [JsonProperty("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonProperty("addr_address")]
        public string AddrAddress { get; set; }

        [JsonProperty("aspect")]
        public Aspect[] Aspect { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("formatted_phone_number")]
        public string FormattedPhoneNumber { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("international_phone_number")]
        public string InternationalPhoneNumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("photos")]
        public Photo[] Photos { get; set; }

        [JsonProperty("opening_hours")]
        public OpeningHours OpeningHours { get; set; }

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

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("user_ratings_total")]
        public int UserRatingsTotal { get; set; }

        [JsonProperty("utc_offset")]
        public int UtcOffset { get; set; }

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }
    }

    public partial class AddressComponent
    {
        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }
    }

    public partial class ViewPort
    {
        [JsonProperty("northeast")]
        public NorthEast NorthEast { get; set; }

        [JsonProperty("southwest")]
        public SouthWest SouthWest { get; set; }
    }

    public partial class NorthEast
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public partial class SouthWest
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public partial class GeocodePlaceDetails
    {
        public static GeocodePlaceDetails FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GeocodePlaceDetails>(json, Converter.Settings);
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

