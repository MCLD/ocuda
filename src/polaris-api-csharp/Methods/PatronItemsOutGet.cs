using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Get a list of a patron's checkouts
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="password"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public PapiResponse<PatronItemsOutGetResult> PatronItemsOutGet(string barcode, string password, PatronItemsOutGetStatus status)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/itemsout/{status}";
            return Execute<PatronItemsOutGetResult>(HttpMethod.Get, url, password);
        }

        /// <summary>
        /// Get a list of a patron's checkouts as a staff member
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public PapiResponse<PatronItemsOutGetResult> PatronItemsOutGetOverride(string barcode, PatronItemsOutGetStatus status)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/itemsout/{status}";
            return OverrideExecute<PatronItemsOutGetResult>(HttpMethod.Get, url);
        }
    }
}