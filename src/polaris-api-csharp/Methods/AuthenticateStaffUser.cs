using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clc.Polaris.Api
{
    public partial class PapiClient
    {
        /// <summary>
        /// Create a protected token to use to call protected methods
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public PapiResponse<ProtectedToken> AuthenticateStaffUser(string domain, string username, string password)
        {
            var url = "/PAPIService/REST/protected/v1/1033/100/1/authenticator/staff";

            var doc = new XDocument(
                new XElement("AuthenticationData",
                    new XElement("Domain", domain),
                    new XElement("Username", username),
                    new XElement("Password", password)
                    )
                );

            var xml = doc.ToString();
            var result = Execute<ProtectedToken>(HttpMethod.Post, url, null, body: xml);
            return result;
        }
    }
}
