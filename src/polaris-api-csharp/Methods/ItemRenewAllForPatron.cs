using Clc.Polaris.Api.Models;

namespace Clc.Polaris.Api
{
	public partial class PapiClient
	{
        /// <summary>
        /// Uses the supplied patron credentials to renew all of their items.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="password">The patron's password.</param>
        /// <param name="options">Item renew options</param>
        /// <returns>An item containing the result of the renewal.</returns>
        /// <seealso cref="ItemsOutActionResult"/>
        public PapiResponse<ItemsOutActionResult> ItemRenewAllForPatron(string barcode, string password, ItemRenewOptions options = null)
		{
            return ItemRenew(barcode, password, 0, options);
		}

        /// <summary>
        /// Renews all of a patron's items as a staff member.
        /// </summary>
        /// <param name="barcode">The patron's barcode.</param>
        /// <param name="options">Item renew options</param>
        /// <returns>An item containing the result of the renewal.</returns>
        /// <seealso cref="ItemsOutActionResult"/>
        public PapiResponse<ItemsOutActionResult> staff_ItemRenewAllForPatron(string barcode, ItemRenewOptions options = null)
		{
            return ItemRenewOverride(barcode, 0, options);
		}
	}
}