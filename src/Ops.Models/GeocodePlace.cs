using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using BranchLocator.Models.PlaceDetails;

namespace BranchLocator.Models
{
    public partial class GeocodePlace
    {
        [JsonProperty("html_attributions")]
        public string[] HtmlAtrributions { get; set; }

        [JsonProperty("results")]
        public PlaceResult[] Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class PlaceResult
    {
        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("photos")]
        public Photo[] Photos { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }


    }
    public partial class Geometry
    {
        [JsonProperty("location")]
        public Locate Locate { get; set; }

        [JsonProperty("viewport")]
        public ViewPort Viewport { get; set; }

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

        [JsonProperty("periods")]
        public Period[] Periods { get; set; }

        [JsonProperty("weekday_text")]
        public string[] GetWeekDayText { get; set; }
    }

    public partial class Period
    {
        [JsonProperty("open")]
        public Open Open { get; set; }

        [JsonProperty("close")]
        public Close Close { get; set; }


    }
    public partial class Open
    {
        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

    }
    public partial class Close
    {
        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

    }
    public partial class Aspect
    {
        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }

    public partial class GeocodePlace
    {
        public static GeocodePlace FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GeocodePlace>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this GeocodePlace self)
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

