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
        public PapiResponse<DatesClosedGetResult> DatesClosedGet(int organizationId)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/{organizationId}/datesclosed";
            return Execute<DatesClosedGetResult>(HttpMethod.Get, url);
        }
    }
}
