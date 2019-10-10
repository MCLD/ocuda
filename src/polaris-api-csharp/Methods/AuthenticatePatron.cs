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
        public PapiResponse<PatronAuthenticationResult> AuthenticatePatron(string barcode, string password)
        {
            var url = "/PAPIService/REST/public/v1/1033/100/1/authenticator/patron";
            var body = $"<PatronAuthenticationData><Barcode>{barcode}</Barcode><Password>{password}</Password></PatronAuthenticationData>";
            return Execute<PatronAuthenticationResult>(HttpMethod.Post, url, body: body);
        }
    }
}
