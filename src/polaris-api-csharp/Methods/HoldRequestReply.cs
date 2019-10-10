using Clc.Polaris.Api.Helpers;
using Clc.Polaris.Api.Models;
using System.Net.Http;
using System.Xml.Linq;


namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Replies as a patron to a conditonal raised by a previous hold request creation.
		/// </summary>
		/// <param name="holdRequest">The result of the hold request that you are replying to.</param>
		/// <param name="requestingOrgId">The Org ID of the branch processing the request.</param>
		/// <param name="answer">The answer to the conditional.</param>
		/// <param name="state">Which conditional will this answer be replied to?</param>
		/// <param name="password">The patron's password.</param>
		/// <returns>An object containing the hold request creation result.</returns>
		/// <seealso cref="HoldRequestReplyData"/>
		/// <seealso cref="HoldRequestResult"/>
		public PapiResponse<HoldRequestReplyData> HoldRequestReply(HoldRequestResult holdRequest, int requestingOrgId, int answer, int state, string password)
		{

            var xml = HoldRequestHelper.BuildHoldRequestReplyXml(holdRequest, requestingOrgId, answer, state);
            var url = $"/PAPIService/REST/public/v1/1033/100/1/holdrequest/{holdRequest.RequestGuid}";
			return Execute<HoldRequestReplyData>(HttpMethod.Put, url, password, xml);
		}

        /// <summary>
        /// Replies as a staff member on the behalf of a patron to a conditonal raised by a previous hold request creation.
        /// </summary>
        /// <param name="holdRequest">The result of the hold request that you are replying to.</param>
        /// <param name="requestingOrgId">The Org ID of the branch processing the request.</param>
        /// <param name="answer">The answer to the conditional.</param>
        /// <param name="state">Which conditional will this answer be replied to?</param>
        /// <returns>An object containing the hold request creation result.</returns>
        /// <seealso cref="HoldRequestReplyData"/>
        /// <seealso cref="HoldRequestResult"/>
        public PapiResponse<HoldRequestReplyData> HoldRequestReply(HoldRequestResult holdRequest, int requestingOrgId, int answer, int state)
        {
            var xml = HoldRequestHelper.BuildHoldRequestReplyXml(holdRequest, requestingOrgId, answer, state);
            var url = $"/PAPIService/REST/public/v1/1033/100/1/holdrequest/{holdRequest.RequestGuid}";
            return OverrideExecute<HoldRequestReplyData>(HttpMethod.Put, url, xml);
        }
    }
}