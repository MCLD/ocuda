using Clc.Polaris.Api.Models;
using System.Linq;
using System.Net.Http;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
		/// <summary>
		/// Gets the barcode for the supplied PatronID.
		/// </summary>
		/// <param name="patronId">The patron's PatronID</param>
		/// <returns>The patron's barcode.</returns>
		public PapiResponse<GetBarcodeAndPatronIDResult> Patron_GetBarcodeFromID(int patronId)
		{
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/patron/barcode?patronid={patronId}";
            return Execute<GetBarcodeAndPatronIDResult>(HttpMethod.Get, url, Token.AccessSecret);
		}
	}
}