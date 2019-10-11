using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clc.Polaris.Api.Helpers
{
    /// <summary>
    /// Helper methods for PAPI patron registration
    /// </summary>
    public class PatronRegistrationHelper
    {
        /// <summary>
        /// Build the XML for a PAPI patron registration
        /// </summary>
        /// <param name="_params"></param>
        /// <returns></returns>
        public static string BuildXml(PatronRegistrationParams _params)
        {
            var doc = new XDocument();
            var root = new XElement("PatronRegistrationCreateData");

            var t = _params.GetType();
            foreach (PropertyInfo info in t.GetProperties())
            {
                var val = info.GetValue(_params, null);

                if (val != null)
                {
                    root.Add(new XElement(info.Name, val != null ? val.GetType() == typeof(DateTime) ? ((DateTime?)val).Value.ToString("s") : val.ToString() : null));
                }
            }

            doc.Add(root);

            return doc.ToString();
        }
    }
}
