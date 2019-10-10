using Clc.Polaris.Api.Helpers;
using Clc.Polaris.Api.Models;
using System;
using System.Net.Http;
using System.Xml.Linq;


namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Uses patron supplied credentials to reactivate a hold request.
		/// </summary>
		/// <param name="barcode">The patron's barcode.</param>
		/// <param name="requestId">The ID of the hold request.</param>
		/// <param name="password">The patron's password.</param>
		/// <param name="activationDate">The date the hold request will become active.</param>
        /// <param name="userId">The ID of the user making the request</param>
		/// <returns>An error code and message from the API, if any.</returns>
		/// <seealso cref="HoldRequestCancelResult"/>
		public PapiResponse<HoldRequestActivationResult> HoldRequestReactivate(string barcode, string password, int requestId, DateTime activationDate, int userId = 1)
        {
            var xml = HoldRequestHelper.BuildActivationXml(userId, activationDate);
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{requestId}/active";
			return Execute<HoldRequestActivationResult>(HttpMethod.Put, url, password, xml);
		}

        /// <summary>
        /// Uses staff credentials to reactivate a hold request for a user.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="requestId">The ID of the hold request.</param>
        /// <param name="activationDate">The date the hold request will become active.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        /// <returns>An error code and message from the API, if any.</returns>
        /// <seealso cref="HoldRequestCancelResult"/>
        public PapiResponse<HoldRequestActivationResult> HoldRequestReactivateOverride(string barcode, int requestId, DateTime activationDate, int userId = 1)
        {
            var xml = HoldRequestHelper.BuildActivationXml(userId, activationDate);
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/holdrequests/{requestId}/active";
            return OverrideExecute<HoldRequestActivationResult>(HttpMethod.Put, url, xml);
        }
    }
}