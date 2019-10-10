using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api
{
    public partial class PapiClient
    {
        /// <summary>
        /// Get information about a patron
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="pin"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public PapiResponse<PatronBasicDataGetResult> PatronBasicDataGet(string barcode, string pin, bool addresses = false)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/basicdata?addresses={Convert.ToInt32(addresses)}";
            return Execute<PatronBasicDataGetResult>(HttpMethod.Get, url, barcode, pin);
        }

        /// <summary>
        /// Get information about a patron as a staff member
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public PapiResponse<PatronBasicDataGetResult> PatronBasicDataGetOverride(string barcode, bool addresses = false)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/basicdata?addresses={Convert.ToInt32(addresses)}";
            return OverrideExecute<PatronBasicDataGetResult>(HttpMethod.Get, url);
        }
    }
}
