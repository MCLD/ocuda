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
        /// Get a list of a patron's fines and fees
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public PapiResponse<PatronAccountGetResult> PatronAccountGet(string barcode, string pin)
        {
            string url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/account/outstanding";
            return Execute<PatronAccountGetResult>(HttpMethod.Get, url, pin);
        }

        /// <summary>
        /// Get a list of a patron's fines and fees as a staff member
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public PapiResponse<PatronAccountGetResult> PatronAccountGetOverride(string barcode)
        {
            string url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/account/outstanding";
            return OverrideExecute<PatronAccountGetResult>(HttpMethod.Get, url);
        }
    }
}
