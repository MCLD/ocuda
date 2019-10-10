using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of a PatronAccountGet call
    /// </summary>
    [XmlRoot(ElementName = "PatronAccountGetResult")]
    public class PatronAccountGetResult : PapiResponseCommon
    {
        /// <summary>
        /// Rows of patron fine and fee information
        /// </summary>
        [XmlElement(ElementName = "PatronAccountGetRows")]
        public PatronAccountGetRows PatronAccountGetRows { get; set; }
    }

    /// <summary>
    /// Container for patron account rows
    /// </summary>
    [XmlRoot(ElementName = "PatronAccountGetRows")]
    public class PatronAccountGetRows
    {
        /// <summary>
        /// List of patron fine and fee information
        /// </summary>
        [XmlElement(ElementName = "PatronAccountGetRow")]
        public List<PatronAccountGetRow> PatronAccountGetRow { get; set; }
    }

    /// <summary>
    /// Patron fine/fee information
    /// </summary>
    [XmlRoot(ElementName = "PatronAccountGetRow")]
    public class PatronAccountGetRow
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        [XmlElement(ElementName = "TransactionID")]
        public string TransactionID { get; set; }

        /// <summary>
        /// Transaction date
        /// </summary>
        [XmlElement(ElementName = "TransactionDate")]
        public string TransactionDate { get; set; }

        /// <summary>
        /// Associated branch
        /// </summary>
        [XmlElement(ElementName = "BranchID")]
        public string BranchID { get; set; }

        /// <summary>
        /// Associated branch
        /// </summary>
        [XmlElement(ElementName = "BranchName")]
        public string BranchName { get; set; }

        /// <summary>
        /// Transaction type id
        /// </summary>
        [XmlElement(ElementName = "TransactionTypeID")]
        public string TransactionTypeID { get; set; }

        /// <summary>
        /// Transaction type description
        /// </summary>
        [XmlElement(ElementName = "TransactionTypeDescription")]
        public string TransactionTypeDescription { get; set; }

        /// <summary>
        /// Fee description
        /// </summary>
        [XmlElement(ElementName = "FeeDescription")]
        public string FeeDescription { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        [XmlElement(ElementName = "TransactionAmount")]
        public string TransactionAmount { get; set; }

        /// <summary>
        /// Outstanding amount
        /// </summary>
        [XmlElement(ElementName = "OutstandingAmount")]
        public string OutstandingAmount { get; set; }

        /// <summary>
        /// Free text note
        /// </summary>
        [XmlElement(ElementName = "FreeTextNote")]
        public string FreeTextNote { get; set; }

        /// <summary>
        /// Item ID
        /// </summary>
        [XmlElement(ElementName = "ItemID")]
        public string ItemID { get; set; }

        /// <summary>
        /// Item barcode
        /// </summary>
        [XmlElement(ElementName = "Barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Item bibiographic record ID
        /// </summary>
        [XmlElement(ElementName = "BibID")]
        public string BibID { get; set; }

        /// <summary>
        /// Material type ID
        /// </summary>
        [XmlElement(ElementName = "FormatID")]
        public string FormatID { get; set; }

        /// <summary>
        /// Material type description
        /// </summary>
        [XmlElement(ElementName = "FormatDescription")]
        public string FormatDescription { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// Author
        /// </summary>
        [XmlElement(ElementName = "Author")]
        public string Author { get; set; }

        /// <summary>
        /// Call number
        /// </summary>
        [XmlElement(ElementName = "CallNumber")]
        public string CallNumber { get; set; }

        /// <summary>
        /// Checkout date
        /// </summary>
        [XmlElement(ElementName = "CheckOutDate")]
        public string CheckOutDate { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        [XmlElement(ElementName = "DueDate")]
        public string DueDate { get; set; }
    }
}
