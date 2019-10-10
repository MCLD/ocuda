using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Contains the result of a bibliographic record search.
	/// </summary>
	public class BibSearchResult : PapiResponseCommon
	{
		/// <summary>
		/// List of words used to perform the search.
		/// </summary>
		public string WordList { get; set; }

		/// <summary>
		/// Total number of records that match the search criteria.
		/// </summary>
		public int TotalRecordsFound { get; set; }

		/// <summary>
		/// A row that contains bibliographic record information of the search results.
		/// </summary>
		public List<BibSearchRow> BibSearchRows { get; set; }
	}

	/// <summary>
	/// A representation of the information contained in the bibliographic record.
	/// </summary>
	public class BibSearchRow
	{
		/// <summary>
		/// Position of this result in the list.
		/// </summary>
		public int Position { get; set; }

		/// <summary>
		/// BibliographicRecordID of this record.
		/// </summary>
		public int ControlNumber { get; set; }

		/// <summary>
		/// Author of this record.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Title of this record.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Publisher of this record.
		/// </summary>
		public string Publisher { get; set; }

		/// <summary>
		/// Edition of this record.
		/// </summary>
		public string Edition { get; set; }

		/// <summary>
		/// Publication date of this record.
		/// </summary>
		public string PublicationDate { get; set; }

		/// <summary>
		/// ISBN of this record.
		/// </summary>
		public string ISBN { get; set; }

		/// <summary>
		/// Description of this record.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Web link of this record.
		/// </summary>
		public string WebLink { get; set; }

		/// <summary>
		/// GUESS: If this record is a work of fiction.
		/// </summary>
		public string Fiction { get; set; }

		/// <summary>
		/// Type of material of this record.
		/// </summary>
		public string TypeOfMaterial { get; set; }

		/// <summary>
		/// Call number of this record.
		/// </summary>
		public string CallNumber { get; set; }

		/// <summary>
		/// GUESS: Number of reserves of courses.
		/// </summary>
		public int CourseReserveCount { get; set; }

		/// <summary>
		/// Number of local items in.
		/// </summary>
		public int LocalItemsIn { get; set; }

		/// <summary>
		/// Total number of local items.
		/// </summary>
		public int LocalItemsTotal { get; set; }

		/// <summary>
		/// Number of items in system wide.
		/// </summary>
		public int SystemItemsIn { get; set; }

		/// <summary>
		/// Total number of items system wide.
		/// </summary>
		public int SystemItemsTotal { get; set; }

		/// <summary>
		/// Current number of hold requests.
		/// </summary>
		public int CurrentHoldRequests { get; set; }

		/// <summary>
		/// Key word in context.
		/// </summary>
		public string KWIC { get; set; }

		/// <summary>
		/// Retention statement of this record.
		/// </summary>
		public string RetentionStatement { get; set; }

		/// <summary>
		/// Holdings statement of this record.
		/// </summary>
		public string HoldingsStatement { get; set; }

		/// <summary>
		/// Holdings note of this record.
		/// </summary>
		public string HoldingsNote { get; set; }

		/// <summary>
		/// Summary of this record.
		/// </summary>
		public string Summary { get; set; }

		/// <summary>
		/// GUESS: OCLC number of this record.
		/// </summary>
		public string OCLC { get; set; }

		/// <summary>
		/// UPC number of this record.
		/// </summary>
		public string UPC { get; set; }

		/// <summary>
		/// Target audience of this record.
		/// </summary>
		public string TargetAudience { get; set; }

		/// <summary>
		/// Medium of this record.
		/// </summary>
		public string Medium { get; set; }

		/// <summary>
		/// Thumbnail link for this record.
		/// </summary>
		public string ThumbnailLink { get; set; }

		/// <summary>
		/// Vernacular title of this record.
		/// </summary>
		public string VernacularTitle { get; set; }

		/// <summary>
		/// Vernacular author of this record.
		/// </summary>
		public string VernacularAuthor { get; set; }

		/// <summary>
		/// Vernacular publisher of this record.
		/// </summary>
		public string VernacularPublisher { get; set; }

		/// <summary>
		/// Local control number of this record.
		/// </summary>
		public string LocalControlNumber { get; set; }

		/// <summary>
		/// Series of this record.
		/// </summary>
		public string Series { get; set; }

		/// <summary>
		/// Series suggested query for this record.
		/// </summary>
		public string SeriesSuggestedQuery { get; set; }

		/// <summary>
		/// Vernacular series of this record.
		/// </summary>
		public string VernacularSeries { get; set; }
	}
}