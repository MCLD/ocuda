using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Returns holdings information for a supplied record.
		/// </summary>
		/// <param name="bibId">BibliograhpicRecordID of the record.</param>
		/// <returns>An object containing a list of holdings information for specified BibliographicRecordID.</returns>
		/// <seealso cref="BibHoldingsGetResult"/>
		public PapiResponse<BibHoldingsGetResult> HoldingsGet(int bibId)
		{
            var url = $"/PAPIService/REST/public/v1/1033/100/1/bib/{bibId}/holdings";
            return Execute<BibHoldingsGetResult>(HttpMethod.Get, url);
		}
	}
}