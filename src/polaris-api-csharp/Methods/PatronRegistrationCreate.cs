using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

using Clc.Polaris.Api.Validation;
using System;
using Clc.Polaris.Api.Models;
using Clc.Polaris.Api.Helpers;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Create a patron registration
        /// </summary>
        /// <param name="_params"></param>
        /// <returns></returns>
        public PapiResponse<PatronRegistrationCreateResult> PatronRegistrationCreate(PatronRegistrationParams _params)
        {
            Require.Argument("PatronBranchID", _params.PatronBranchID);
            Require.Argument("NameFirst", _params.NameFirst);
            Require.Argument("NameLast", _params.NameLast);

            var xml = PatronRegistrationHelper.BuildXml(_params);
            var url = "/PAPIService/REST/public/v1/1033/100/1/patron";
            return Execute<PatronRegistrationCreateResult>(HttpMethod.Post, url, null, xml);
        }
	}
}