using Clc.Polaris.Api.Helpers;
using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Updates information in the patron's record.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="password">The patron's password.</param>
        /// <param name="updateParams">Contains the values to update the patron's record with</param>
        public PapiResponse<PatronUpdateResult> PatronUpdate(string barcode, string password, PatronUpdateParams updateParams)
		{
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}";
            var xml = PatronUpdateHelper.BuildXml(updateParams);
            return Execute<PatronUpdateResult>(HttpMethod.Put, url, password, xml);
		}

        /// <summary>
        /// Updates information in the patron's record
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="updateParams">Contains the values to update the patron's record with</param>
        public PapiResponse<PatronUpdateResult> PatronUpdateOverride(string barcode, PatronUpdateParams updateParams)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}";
            var xml = PatronUpdateHelper.BuildXml(updateParams);
            return OverrideExecute<PatronUpdateResult>(HttpMethod.Put, url, xml);
        }
    }
}
