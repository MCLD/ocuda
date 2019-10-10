namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The available search sorting options.
	/// </summary>
	public enum SearchSortOptions
	{
		/// <summary>
		/// Relevance
		/// </summary>
		RELEVANCE,

		/// <summary>
		/// Most Popular
		/// </summary>
		MP,

		/// <summary>
		/// Author
		/// </summary>
		AU,

		/// <summary>
		/// Title
		/// </summary>
		TI,

		/// <summary>
		/// Call Number
		/// </summary>
		CALL,

		/// <summary>
		/// Publication Date Descending
		/// </summary>
		PD,

		/// <summary>
		/// Author then Title
		/// </summary>
		AUTI,

		/// <summary>
		/// Author then Publication Date Descending
		/// </summary>
		AUPD,

		/// <summary>
		/// Title then Author
		/// </summary>
		TIAU,

		/// <summary>
		/// Title then Publication Date Descending
		/// </summary>
		TIPD,

		/// <summary>
		/// Publication Date Descending then Author
		/// </summary>
		PDAU,

		/// <summary>
		/// Publication Date Descending then Title
		/// </summary>
		PDTI,

		/// <summary>
		/// Call Number then Author
		/// </summary>
		CALLAU,

		/// <summary>
		/// Call Number then Title
		/// </summary>
		CALLTI,

		/// <summary>
		/// Call Number then Publication Date Descending
		/// </summary>
		CALLPD
	}
}