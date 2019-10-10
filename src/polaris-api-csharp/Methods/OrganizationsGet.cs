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
        /// Get a list of organizations
        /// </summary>
        /// <param name="type">Organization type</param>
        /// <returns></returns>
        public PapiResponse<OrganizationsGetResult> OrganizationsGet(OrganizationType type)
        {
            return Execute<OrganizationsGetResult>(HttpMethod.Get, $"/PAPIService/REST/public/v1/1033/100/1/organizations/{type}");
        }
    }
}
