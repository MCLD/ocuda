

using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Uses patron supplied credentials to cancel a hold request.
		/// </summary>
		/// <param name="barcode">The patron's barcode.</param>
		/// <param name="requestId">The ID of the hold request.</param>
		/// <param name="password">The patron's password.</param>
		/// <param name="workstationId">The ID of the workstation processing the request.</param>
		/// <param name="userId">The ID of the user processing the request..</param>
		/// <returns>An error code and message from the API, if any.</returns>
		/// <seealso cref="HoldRequestCancelResult"/>
		public PapiResponse<HoldRequestCancelResult> HoldRequestCancel(string barcode, string password, int requestId, int workstationId = 1, int userId = 1)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{requestId}/cancelled?wsid={workstationId}&userid={userId}";
			return Execute<HoldRequestCancelResult>(HttpMethod.Put, url, password);
		}

		/// <summary>
		/// Uses staff credentials to cancel a hold request for a user.
		/// </summary>
		/// <param name="barcode">The patron's barcode.</param>
		/// <param name="requestId">The ID of the hold request.</param>
		/// <param name="workstationId">The ID of the workstation processing the request.</param>
		/// <param name="userId">The ID of the user processing the request.</param>
		/// <returns>An error code and message from the API, if any.</returns>
		/// <seealso cref="HoldRequestCancelResult"/>
		public PapiResponse<HoldRequestCancelResult> HoldRequestCancelOverride(string barcode, int requestId, int workstationId = 1, int userId = 1)
		{
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{requestId}/cancelled?wsid={workstationId}&userid={userId}";
            return OverrideExecute<HoldRequestCancelResult>(HttpMethod.Put, url);
        }
	}
}