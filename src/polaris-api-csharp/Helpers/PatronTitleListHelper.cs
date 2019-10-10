using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clc.Polaris.Api.Helpers
{
    /// <summary>
    /// Helper methods for patron title list methods
    /// </summary>
    public static class PatronTitleListHelper
    {
        /// <summary>
        /// Build the XML for a PatronTitleListAddTitle call
        /// </summary>
        /// <param name="recordStoreId"></param>
        /// <param name="recordName"></param>
        /// <param name="localControlNumber"></param>
        /// <returns></returns>
        public static string BuildAddTitleXml(int recordStoreId, string recordName, int localControlNumber)
        {
            var doc = new XDocument(
                new XElement("PatronTitleListAddTitleData",
                             new XElement("RecordStoreID", recordStoreId),
                             new XElement("RecordName", recordName),
                             new XElement("LocalControlNumber", localControlNumber)
                    )
                );
            return doc.ToString();
        }
    }
}
