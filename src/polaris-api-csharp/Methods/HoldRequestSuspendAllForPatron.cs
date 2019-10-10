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
        public PapiResponse<HoldRequestActivationResult> HoldRequestSuspendAll(string barcode, string password, DateTime activationDate, int userId = 1)
        {
            return HoldRequestSuspend(barcode, password, 0, activationDate, userId);
        }

        /// <summary>
        /// Uses staff credentials to suspend a hold request for a user.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="activationDate">The date the hold request will become active.</param>
        /// <returns>An error code and message from the API, if any.</returns>
		/// <param name="userId">The ID of the user processing the request..</param>
        /// <seealso cref="HoldRequestCancelResult"/>
        public PapiResponse<HoldRequestActivationResult> HoldRequestSuspendAllOverride(string barcode, DateTime activationDate, int userId = 1)
        {
            return HoldRequestSuspendOverride(barcode, 0, activationDate, userId);
        }
    }
}