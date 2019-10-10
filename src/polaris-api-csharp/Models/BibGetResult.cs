using System.Collections.Generic;
using System.Linq;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Contains a list of elements representing data contained in the MARC record.
	/// </summary>
	public class BibGetResult : PapiResponseCommon
	{
		/// <summary>
		/// List of rows containing raw bibliographic record information.
		/// </summary>
		public List<BibGetRow> BibGetRows { get; set; }

		/// <summary>
		/// Publisher(s) of the record.
		/// </summary>
		public List<string> Publisher
		{
			get { return GetBibResultRow(2); }
		}

		/// <summary>
		/// Description(s) of the record.
		/// </summary>
		public List<string> Description
		{
			get { return GetBibResultRow(3); }
		}

		/// <summary>
		/// Edition(s) of the record.
		/// </summary>
		public List<string> Edition
		{
			get { return GetBibResultRow(5); }
		}

		/// <summary>
		/// ISBN of the record.
		/// </summary>
		public string ISBN
		{
			get { return GetBibResultRow(6).FirstOrDefault(); }
		}

		/// <summary>
		/// Number of items associated with this record system-wide.
		/// </summary>
		public int? SystemItemsTotal
		{
			get
			{
				int dummy;
				return int.TryParse(GetBibResultRow(7).FirstOrDefault() + "", out dummy) ? dummy : new int?();
			}
		}

		/// <summary>
		/// Current number of holds on this record.
		/// </summary>
		public int? CurrentHolds
		{
			get
			{
				int dummy;
				return int.TryParse(GetBibResultRow(8).FirstOrDefault() + "", out dummy) ? dummy : new int?();
			}
		}

		/// <summary>
		/// Summary(ies) of this record.
		/// </summary>
		public List<string> Summary
		{
			get { return GetBibResultRow(9); }
		}

		/// <summary>
		/// Number of local items associated with this record.
		/// </summary>
		public List<string> LocalItemsTotal
		{
			get { return GetBibResultRow(10); }
		}

		/// <summary>
		/// Control number of this record.
		/// </summary>
		public int? ControlNumber
		{
			get
			{
				int dummy;
				return int.TryParse(GetBibResultRow(11).FirstOrDefault() + "", out dummy) ? dummy : new int?();
			}
		}

		/// <summary>
		/// Call number(s) of this record.
		/// </summary>
		public List<string> CallNumber
		{
			get { return GetBibResultRow(13); }
		}

		/// <summary>
		/// Number of local items available that are associated with this record.
		/// </summary>
		public int? LocalItemsAvailable
		{
			get
			{
				int dummy;
				return int.TryParse(GetBibResultRow(15).FirstOrDefault() + "", out dummy) ? dummy : new int?();
			}
		}

		/// <summary>
		/// Number of available items associated with this record system-wide.
		/// </summary>
		public int? SystemItemsAvailable
		{
			get
			{
				int dummy;
				return int.TryParse(GetBibResultRow(16).FirstOrDefault() + "", out dummy) ? dummy : new int?();
			}
		}

		/// <summary>
		/// Format of the record.
		/// </summary>
		public string Format
		{
			get { return GetBibResultRow(17).FirstOrDefault(); }
		}

		/// <summary>
		/// Author of the record.
		/// </summary>
		public List<string> Author
		{
			get { return GetBibResultRow(18); }
		}

		/// <summary>
		/// Series of the record.
		/// </summary>
		public List<string> Series
		{
			get { return GetBibResultRow(19); }
		}

		/// <summary>
		/// Subject of the record.
		/// </summary>
		public List<string> Subject
		{
			get { return GetBibResultRow(20); }
		}

		/// <summary>
		/// Added author of the record.
		/// </summary>
		public List<string> AddedAuthor
		{
			get { return GetBibResultRow(21); }
		}

		/// <summary>
		/// Added title of the record.
		/// </summary>
		public List<string> AddedTitle
		{
			get { return GetBibResultRow(22); }
		}

		/// <summary>
		/// LCCN of the record.
		/// </summary>
		public string LCCN
		{
			get { return GetBibResultRow(23).FirstOrDefault(); }
		}

		/// <summary>
		/// ISSN of the record.
		/// </summary>
		public string ISSN
		{
			get { return GetBibResultRow(24).FirstOrDefault(); }
		}

		/// <summary>
		/// Other number of the record.
		/// </summary>
		public string OtherNumber
		{
			get { return GetBibResultRow(25).FirstOrDefault(); }
		}

		/// <summary>
		/// Genre of the title.
		/// </summary>
		public List<string> Genre
		{
			get { return GetBibResultRow(27); }
		}

		/// <summary>
		/// Notes on the record.
		/// </summary>
		public List<string> Notes
		{
			get { return GetBibResultRow(28); }
		}

		/// <summary>
		/// Contents of the record.
		/// </summary>
		public List<string> Contents
		{
			get { return GetBibResultRow(29); }
		}

		/// <summary>
		/// Publisher number of the record.
		/// </summary>
		public List<string> PublisherNumber
		{
			get { return GetBibResultRow(30); }
		}

		/// <summary>
		/// Web link of the record.
		/// </summary>
		public List<string> WebLink
		{
			get { return GetBibResultRow(33); }
		}

		/// <summary>
		/// Uniform title of the record.
		/// </summary>
		public string UniformTitle
		{
			get { return GetBibResultRow(34).FirstOrDefault(); }
		}

		/// <summary>
		/// Title of the record.
		/// </summary>
		public string Title
		{
			get { return GetBibResultRow(35).FirstOrDefault(); }
		}

		/// <summary>
		/// Volume of the record.
		/// </summary>
		public string Volume
		{
			get { return GetBibResultRow(36).FirstOrDefault(); }
		}

		/// <summary>
		/// Frequency of the record.
		/// </summary>
		public string Frequency
		{
			get { return GetBibResultRow(37).FirstOrDefault(); }
		}

		/// <summary>
		/// Former title of the record.
		/// </summary>
		public List<string> FormerTitle
		{
			get { return GetBibResultRow(39); }
		}

		/// <summary>
		/// Later title of the record.
		/// </summary>
		public List<string> LaterTitle
		{
			get { return GetBibResultRow(40); }
		}

		/// <summary>
		/// STRN of the record.
		/// </summary>
		public List<string> STRN
		{
			get { return GetBibResultRow(41); }
		}

		/// <summary>
		/// GPO item number of the record.
		/// </summary>
		public List<string> GPOItemNumber
		{
			get { return GetBibResultRow(42); }
		}

		/// <summary>
		/// CODEN of the record.
		/// </summary>
		public List<string> CODEN
		{
			get { return GetBibResultRow(43); }
		}

		/// <summary>
		/// Target audience of the record.
		/// </summary>
		public List<string> TargetAudience
		{
			get { return GetBibResultRow(44); }
		}

		/// <summary>
		/// Medium of the record.
		/// </summary>
		public string Medium
		{
			get { return GetBibResultRow(46).FirstOrDefault(); }
		}


		private List<string> GetBibResultRow(int id)
		{
			if (BibGetRows.Any(b => b.ElementID == id))
			{
				return BibGetRows.Where(b => b.ElementID == id).Select(b => b.Value).ToList();
			}
			return new List<string>();
		}
	}


	/// <summary>
	/// Contains a the value of a bibliographic record field.
	/// </summary>
	public class BibGetRow
	{
		/// <summary>
		/// The element identifier.
		/// </summary>
		public int ElementID { get; set; }

		/// <summary>
		/// The occurrence of a given element matches the relative order of the underlying data in the MARC record. 
		/// </summary>
		public int Occurrence { get; set; }

		/// <summary>
		/// The label associated with this element.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The value of the element.
		/// </summary>
		public string Value { get; set; }

        public override string ToString()
        {
            return $"{Label} - {Value}";
        }
    }
}