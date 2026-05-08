using System.Text.Json.Serialization;

namespace Ocuda.Models
{
    public class TrestleError
    {
        [JsonPropertyName("message")]
        private string? Message { get; set; }

        [JsonPropertyName("name")]
        private string? Name { get; set; }
    }

    public class TrestleLatLon
    {
        [JsonPropertyName("accuracy")]
        private string? Accuracy { get; set; }

        [JsonPropertyName("latitude")]
        private double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        private double? Longitude { get; set; }
    }

    public class TrestleResponse
    {
        [JsonPropertyName("age_range")]
        public string? AgeRange { get; set; }

        [JsonPropertyName("alternate_names")]
        public string[]? AlternateNames { get; set; }

        [JsonPropertyName("alternate_phones")]
        public TrestleResponse[]? AlternatePhones { get; set; }

        [JsonPropertyName("associated_addresses")]
        public TrestleResponse[]? AssociatedAddresses { get; set; }

        [JsonPropertyName("associated_people")]
        public TrestleResponse[]? AssociatedPeople { get; set; }

        [JsonPropertyName("belongs_to")]
        public TrestleResponse? BelongsTo { get; set; }

        [JsonPropertyName("carrier")]
        public string? Carrier { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("country_calling_code")]
        public string? CountryCallingCode { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("country_name")]
        public string? CountryName { get; set; }

        [JsonPropertyName("current_addresses")]
        public TrestleResponse[]? CurrentAddresses { get; set; }

        [JsonPropertyName("current_residents")]
        public TrestleResponse[]? CurrentResidents { get; set; }

        [JsonPropertyName("delivery_point")]
        public string? DeliveryPoint { get; set; }

        [JsonPropertyName("error")]
        public TrestleError[]? Error { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("found_at_address")]
        public TrestleResponse? FoundAtAddress { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("historical_addresses")]
        public TrestleResponse[]? HistoricalAddresses { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("is_active")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("is_commercial")]
        public bool? IsCommercial { get; set; }

        [JsonPropertyName("is_forwarder")]
        public bool? IsForwarder { get; set; }

        [JsonPropertyName("is_prepaid")]
        public bool? IsPrepaid { get; set; }

        [JsonPropertyName("is_valid")]
        public bool? IsValid { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("last_sale_date")]
        public string? LastSaleDate { get; set; }

        [JsonPropertyName("lat_long")]
        public TrestleLatLon? LatLong { get; set; }

        [JsonPropertyName("line_type")]
        public string? LineType { get; set; }

        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("owners")]
        public TrestleResponse[]? Owners { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("phones")]
        public TrestleResponse[]? Phones { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("state_code")]
        public string? StateCode { get; set; }

        [JsonPropertyName("street_line_1")]
        public string? StreetLine1 { get; set; }

        [JsonPropertyName("street_line_2")]
        public string? StreetLine2 { get; set; }

        [JsonPropertyName("total_value")]
        public int? TotalValue { get; set; }

        [JsonPropertyName("warnings")]
        public string[]? Warnings { get; set; }

        [JsonPropertyName("zip4")]
        public string? Zip4 { get; set; }
    }
}