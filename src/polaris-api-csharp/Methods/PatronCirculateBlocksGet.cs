using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;


namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Get block information about a patron
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
		public PapiResponse<PatronCirculateBlocksResult> PatronCirculateBlocksGet(string barcode, string password)
		{
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/circulationblocks";
            return Execute<PatronCirculateBlocksResult>(HttpMethod.Get, url, password);
        }

        /// <summary>
        /// Get block information about a patron as a staff member
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public PapiResponse<PatronCirculateBlocksResult> PatronCirculateBlocksGetOverride(string barcode)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/circulationblocks";
            return OverrideExecute<PatronCirculateBlocksResult>(HttpMethod.Get, url);
        }
    }
}
