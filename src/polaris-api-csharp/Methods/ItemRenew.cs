using Clc.Polaris.Api.Helpers;
using Clc.Polaris.Api.Models;
using System.Net.Http;
using System.Xml.Linq;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Uses the supplied patron credentials to renew an item.
        /// </summary>
        /// <param name="patronBarcode">The patron's barcode.</param>
        /// <param name="password">The patron's password.</param>
        /// <param name="itemId">The ID of the item to renew, NOT barcode.</param>
        /// <param name="options">Item renew options</param>
        /// <returns>An item containing the result of the item renewal.</returns>
        /// <seealso cref="ItemsOutActionResult"/>
        public PapiResponse<ItemsOutActionResult> ItemRenew(string patronBarcode, string password, int itemId, ItemRenewOptions options = null)
        {
            if (options == null) options = new ItemRenewOptions();
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{patronBarcode}/itemsout/{itemId}";
            var xml = ItemRenewHelper.BuildXml(options);
            return Execute<ItemsOutActionResult>(HttpMethod.Put, url, password, xml);
        }

        /// <summary>
        /// Renews an item for a patron as a staff member.
        /// </summary>
        /// <param name="patronBarcode">The patron's barcode.</param>
        /// <param name="itemId">The ID of the item to renew, NOT barcode.</param>
        /// <param name="options">Item renew options</param>
        /// <returns>An item containing the result of the item renewal.</returns>
        /// <seealso cref="ItemsOutActionResult"/>
        public PapiResponse<ItemsOutActionResult> ItemRenewOverride(string patronBarcode, int itemId, ItemRenewOptions options = null)
        {
            if (options == null) options = new ItemRenewOptions();
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{patronBarcode}/itemsout/{itemId}";
            var xml = ItemRenewHelper.BuildXml(options);
            return OverrideExecute<ItemsOutActionResult>(HttpMethod.Put, url, xml);
        }
    }
}