using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Utility.Models;

namespace Ocuda.Utility.Abstract
{
    public interface IGoogleClient
    {
        Task<(double? Latitude, double? Longitude)> GeocodeAsync(string address);

        Task<string> GetLocationLinkAsync(string placeId);

        Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address);

        Task<string> GetZipCodeAsync(double latitude, double longitude);
    }
}