using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of a PatronCirculateBlocksGet call
    /// </summary>
    public class PatronCirculateBlocksResult : PapiResponseCommon
    {
        /// <summary>
        /// Patron's barcode
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Valid patron
        /// </summary>
        public bool ValidPatron { get; set; }

        /// <summary>
        /// Patron ID
        /// </summary>
        public int PatronID { get; set; }

        /// <summary>
        /// Patron code ID
        /// </summary>
        public int PatronCodeID { get; set; }

        /// <summary>
        /// Assigned branch ID
        /// </summary>
        public int AssignedBranchID { get; set; }

        /// <summary>
        /// The patron's barcode
        /// </summary>
        public string PatronBarcode { get; set; }

        /// <summary>
        /// Assigned branch name
        /// </summary>
        public string AssignedBranchName { get; set; }

        /// <summary>
        /// Account expiration date
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Was the override password used
        /// </summary>
        public bool OverridePasswordUsed { get; set; }

        /// <summary>
        /// List of blocks on the patron's account
        /// </summary>
        ////public Blocks Blocks { get; set; }

        public List<Block> Blocks { get; set; }

        /// <summary>
        /// Can the patron check out items
        /// </summary>
        public bool CanPatronCirculate { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string NameFirst { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string NameLast { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
        public string NameMiddle { get; set; }
    }

    /// <summary>
    /// Patron block information
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Patron ID
        /// </summary>
        public int PatronID { get; set; }

        /// <summary>
        /// Patron name
        /// </summary>
        public string PatronName { get; set; }

        /// <summary>
        /// Block description
        /// </summary>
        public string BlockDescription { get; set; }
    }
}
