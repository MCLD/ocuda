using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of a PatronTitleListAddTitle request
    /// </summary>
    public class PatronTitleListAddTitleResult : PapiResponseCommon
    {
        /// <summary>
        /// Position in the list
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Unique Identifier for the title in the list
        /// </summary>
        public int RecordID { get; set; }
    }
}
