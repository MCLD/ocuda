
using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Uses the supplied patron credentials to retrieve holds for that patron.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="password">The patron's password.</param>
        /// <param name="status">Status of the holds you wish to retrieve.</param>
        /// <returns>PatronHoldRequestsGetResult</returns>
        /// <seealso cref="PatronHoldRequestsGetResult"/>
        /// <seealso cref="HoldStatus"/>
        public PapiResponse<PatronHoldRequestsGetResult> PatronHoldRequestsGet(string barcode, string password, HoldStatus status)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{status}";
            return Execute<PatronHoldRequestsGetResult>(HttpMethod.Get, url, password);
        }

        /// <summary>
        /// Retrieves holds for the supplied patron barcode as a staff member.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="status">Status of the holds you wish to retrieve.</param>
        /// <returns>PatronHoldRequestsGetResult</returns>
        /// <seealso cref="PatronHoldRequestsGetResult"/>
        /// <seealso cref="HoldStatus"/>
        public PapiResponse<PatronHoldRequestsGetResult> PatronHoldRequestsGetOverride(string barcode, HoldStatus status)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{status}";
            return OverrideExecute<PatronHoldRequestsGetResult>(HttpMethod.Get, url);
        }
    }
}