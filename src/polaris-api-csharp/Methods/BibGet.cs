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
        /// Get information about a bibliographic record
        /// </summary>
        /// <param name="bibId"></param>
        /// <returns></returns>
        public PapiResponse<BibGetResult> BibGet(int bibId, int organizationId = 1)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/{organizationId}/bib/{bibId}";
            return Execute<BibGetResult>(HttpMethod.Get, url);
        }
    }
}
