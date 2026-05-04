using System.Text.Json.Serialization;

namespace Ocuda.Models
{
    public class MaricopaCountyAssessorResponse
    {
        public Record[] Results { get; set; }

        [JsonPropertyName("TOTAL")]
        public int Total { get; set; }
    }

    public class Record
    {
        public string APN { get; set; }
        public string MCR { get; set; }
        public string Ownership { get; set; }
        public string PropertyType { get; set; }

        [JsonPropertyName("RentalID")]
        public string? RentalId { get; set; }

        public string SectionTownshipRange { get; set; }
        public string SitusAddress { get; set; }
        public string SitusCity { get; set; }
        public string SitusZip { get; set; }
        public string SubdivisionName { get; set; }
    }
}