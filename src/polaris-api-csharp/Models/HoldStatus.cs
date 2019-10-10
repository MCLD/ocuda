namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// An enum containing valid hold statuses.
	/// </summary>
	public enum HoldStatus
	{
		/// <summary>
		/// All hold requests.
		/// </summary>
		all = 1,

		/// <summary>
		/// Holds with an activation date in the future.
		/// </summary>
		inactive,

		/// <summary>
		/// Holds actively looking to be filled by the system.
		/// </summary>
		active,

		/// <summary>
		/// Requests that can be filled by an item currently 'in' at your library, or the request has been routed to another library that has the item 'in'.
		/// </summary>
		pending,

		/// <summary>
		/// Holds with items that have been trapped at another location and have been shipped to the pickup location.
		/// </summary>
		shipped,

		/// <summary>
		/// Holds with items that have been trapped to fill the request. The held item is ready to be picked up.
		/// </summary>
		held,

		/// <summary>
		/// Holds with no items that can fill the request (for example, a bibliographic record with no attached items).
		/// </summary>
		notsupplied,

		/// <summary>
		/// Holds that have not been picked up by the patron before their pickup date.
		/// </summary>
		unclaimed,

		/// <summary>
		/// Holds that were not filled within the specified amount of time.
		/// </summary>
		expired,

		/// <summary>
		/// Holds that have been cancelled. Cancelled holds can never be filled but may be reactivated or deleted.
		/// </summary>
		cancelled
	}
}