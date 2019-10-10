namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The available search fields.
	/// </summary>
	public enum SearchQualifiers
	{
		/// <summary>
		/// Keyword
		/// </summary>
		KW,

		/// <summary>
		/// Title
		/// </summary>
		TI,

		/// <summary>
		/// Author
		/// </summary>
		AU,

		/// <summary>
		/// Subject
		/// </summary>
		SU,

		/// <summary>
		/// General notes
		/// </summary>
		NOTE,

		/// <summary>
		/// Publisher
		/// </summary>
		PUB,

		/// <summary>
		/// Genre
		/// </summary>
		GENRE,

		/// <summary>
		/// Series
		/// </summary>
		SE,

		/// <summary>
		/// International Standard Book Number
		/// </summary>
		ISBN,

		/// <summary>
		/// International Standard Serial Number
		/// </summary>
		ISSN,

		/// <summary>
		/// Library of Congress Control Number
		/// </summary>
		LCCN,

		/// <summary>
		/// Publisher's number
		/// </summary>
		PN,

		/// <summary>
		/// Library of Congress classification
		/// </summary>
		LC,

		/// <summary>
		/// Dewey classification
		/// </summary>
		DD,

		/// <summary>
		/// Local call number
		/// </summary>
		LOCAL,

		/// <summary>
		/// Superintendent of Documents classification number
		/// </summary>
		SUDOC,

		/// <summary>
		/// Identifier for scientific and technical periodicals
		/// </summary>
		CODEN,

		/// <summary>
		/// Standard Technical Report Number
		/// </summary>
		STRN,

		/// <summary>
		/// Control Number
		/// </summary>
		CN,

		/// <summary>
		/// Barcode
		/// </summary>
		BC
	}
}