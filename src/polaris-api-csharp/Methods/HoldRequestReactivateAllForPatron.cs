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
        /// Uses patron supplied credentials to suspend a hold request.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="activationDate">The date the hold request will become active.</param>
        /// <param name="password">The patron's password.</param>
		/// <param name="userId">The ID of the user processing the request..</param>
        /// <returns>An error code and message from the API, if any.</returns>
        /// <seealso cref="HoldRequestCancelResult"/>
        public PapiResponse<HoldRequestActivationResult> HoldRequestReactivateAll(string barcode, DateTime activationDate, string password, int userId = 1)
        {
            return HoldRequestReactivate(barcode, password, 0, activationDate, userId);
        }

        /// <summary>
        /// Uses staff credentials to suspend a hold request for a user.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="activationDate">The date the hold request will become active.</param>
        /// <param name="password">The patron's password.</param>
		/// <param name="userId">The ID of the user processing the request..</param>
        /// <returns>An error code and message from the API, if any.</returns>
        /// <seealso cref="HoldRequestCancelResult"/>
        public PapiResponse<HoldRequestActivationResult> HoldRequestReactivateAllOverride(string barcode,  DateTime activationDate, string password, int userId = 1)
        {
            return HoldRequestReactivateAllOverride(barcode, activationDate, password, userId);
        }
    }
}