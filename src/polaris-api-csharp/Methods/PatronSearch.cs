

using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Returns a list of users matching the specified CCL
		/// </summary>
		/// <param name="query">The CCL you wish to use to search</param>
		/// <param name="page">The page of results you wish to see, defaults to 1</param>
		/// <param name="pageSize">Maximum number of patrons per page, defaults to 10</param>
		/// <param name="sortBy">The sort order of the return values, defaults to Patron Name</param>
		/// <returns></returns>
		public PapiResponse<PatronSearchResult> PatronSearch(string query, int page = 1, int pageSize = 10, PatronSortKeys sortBy = PatronSortKeys.PATN)
		{
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/search/patrons/Boolean?q={PolarisEncode(query)}&patronsperpage={pageSize}&page={page}&sort={sortBy}";
			return Execute<PatronSearchResult>(HttpMethod.Get, url, Token.AccessSecret);			
		}
	}
}