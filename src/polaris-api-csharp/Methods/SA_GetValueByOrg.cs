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
        /// Get an SA value
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public PapiResponse<StringResult> SA_GetValueByOrg(int orgId, string attribute)
        {
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/organization/{orgId}/sysadmin/attribute/{attribute}";
            return Execute<StringResult>(HttpMethod.Get, url, Token.AccessSecret);
        }
    }
}
