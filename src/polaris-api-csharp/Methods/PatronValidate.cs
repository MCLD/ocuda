

using Clc.Polaris.Api.Models;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Determines whether the supplied barcode and PIN combination is valid.
		/// </summary>
		/// <param name="barcode">The patron's barcode.</param>
		/// <param name="password">The patron's password.</param>
		/// <returns>PatronValidateResult</returns>
		/// <seealso cref="PatronValidateResult"/>
		public PapiResponse<PatronValidateResult> PatronValidate(string barcode, string password)
		{
			var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}";
			return Execute<PatronValidateResult>(HttpMethod.Get, url, password);
		}

        /// <summary>
        /// Determines whether the supplied barcode is a valid patron barcode.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <returns>PatronValidateResult</returns>
        /// <seealso cref="PatronValidateResult"/>
        public PapiResponse<PatronValidateResult> PatronValidateOverride(string barcode)
        {
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}";
            return OverrideExecute<PatronValidateResult>(HttpMethod.Get, url);            
        }
    }
}