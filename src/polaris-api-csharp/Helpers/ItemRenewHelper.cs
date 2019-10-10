using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clc.Polaris.Api.Helpers
{
    /// <summary>
    /// Helper methods for PAPI item renew methods
    /// </summary>
    public class ItemRenewHelper
    {
        /// <summary>
        /// Build the XML for PAPI item renewal
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string BuildXml(ItemRenewOptions options)
        {
            var doc = new XDocument(
                new XElement("ItemsOutActionData",
                    new XElement("Action", "renew"),
                    new XElement("LogonBranchID", options.BranchId),
                    new XElement("LogonUserID", options.UserId),
                    new XElement("LogonWorkstationID", options.WorkstationId),
                    new XElement("RenewData",
                        new XElement("IgnoreOverrideErrors", options.IgnoreOverrideErrors))
                    )
            );
            return doc.ToString();
        }
    }
}
