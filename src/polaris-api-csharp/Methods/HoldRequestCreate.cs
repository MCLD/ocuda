using System;
using System.Xml.Linq;
using Clc.Polaris.Api.Validation;
using Clc.Polaris.Api.Models;
using System.Net.Http;
using Clc.Polaris.Api.Helpers;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Places a hold request on the Bibliographic Record for the Patron as supplied in the HoldRequestCreateParams object. This method places the hold as a patron.
		/// </summary>
		/// <param name="holdParams">The object containing all of the possible options for creating the hold request.</param>
		/// <param name="password">The patron's password.</param>
		/// <returns>An object containing the hold request creation result.</returns>
		/// <seealso cref="HoldRequestCreateParams"/>
		/// <seealso cref="HoldRequestResult"/>
		public PapiResponse<HoldRequestResult> PatronHoldRequestCreate(HoldRequestCreateParams holdParams, string password)
		{
			Require.Argument("PatronID", holdParams.PatronID);
			Require.Argument("BibID", holdParams.BibID);

            string xml = HoldRequestHelper.BuildHoldRequestCreateXml(holdParams);

            var url = "/PAPIService/REST/public/v1/1033/100/1/holdrequest";
            return Execute<HoldRequestResult>(HttpMethod.Post, url, password, xml);
		}

		/// <summary>
		/// Places a hold request on the Bibliographic Record for the Patron as supplied in the HoldRequestCreateParams object. This method places the hold as a staff member.
		/// </summary>
		/// <param name="holdParams">The object containing all of the possible options for creating the hold request.</param>
		/// <returns>An object containing the hold request creation result.</returns>
		/// <seealso cref="HoldRequestCreateParams"/>
		/// <seealso cref="HoldRequestResult"/>
        public PapiResponse<HoldRequestResult> PatronHoldRequestCreate(HoldRequestCreateParams holdParams)
        {
            Require.Argument("PatronID", holdParams.PatronID);
            Require.Argument("BibID", holdParams.BibID);

            string xml = HoldRequestHelper.BuildHoldRequestCreateXml(holdParams);

            var url = "/PAPIService/REST/public/v1/1033/100/1/holdrequest";
            return OverrideExecute<HoldRequestResult>(HttpMethod.Post, url, xml);
        }
	}
}