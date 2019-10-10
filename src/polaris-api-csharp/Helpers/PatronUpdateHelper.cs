using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Clc.Polaris.Api.Helpers
{
    /// <summary>
    /// Contains helper methods for PAPI patron updates
    /// </summary>
    public class PatronUpdateHelper
    {
        /// <summary>
        /// Build the XML for a PAPI patron update
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string BuildXml(PatronUpdateParams options)
        {
            var doc = new XDocument();
            var root = new XElement("PatronUpdateData");

            root.Add(new XElement("LogonBranchID", options.BranchId));
            root.Add(new XElement("LogonUserID", options.UserId));
            root.Add(new XElement("LogonWorkstationID", options.WorkstationId));
            root.AddIfNotNull("ReadingListFlag", options.ReadingListEnabled);
            root.AddIfNotNull("EmailFormat", options.EmailFormat);
            root.AddIfNotNull("DeliveryOptionID", options.DeliveryOptionID);
            root.AddIfNotNull("EmailAddress", options.EmailAddress);
            root.AddIfNotNull("AltEmailAddress", options.AltEmailAddress);
            root.AddIfNotNull("PhoneVoice1", options.PhoneVoice1);
            root.AddIfNotNull("PhoneVoice2", options.PhoneVoice2);
            root.AddIfNotNull("PhoneVoice3", options.PhoneVoice3);
            root.AddIfNotNull("Password", options.NewPassword);
            root.AddIfNotNull("TxtPhoneNumber", options.TxtPhoneNumber);
            root.AddIfNotNull("Phone1CarrierID", options.Phone1CarrierID);
            root.AddIfNotNull("Phone2CarrierID", options.Phone2CarrierID);
            root.AddIfNotNull("Phone3CarrierID", options.Phone3CarrierID);
            if (options.AddressCheckDate.HasValue) root.Add(new XElement("AddrCheckDate", XmlConvert.ToString(options.AddressCheckDate.Value, XmlDateTimeSerializationMode.Utc)));
            if (options.ExpirationDate.HasValue) root.Add(new XElement("ExpirationDate", XmlConvert.ToString(options.ExpirationDate.Value, XmlDateTimeSerializationMode.Utc)));


            doc.Add(root);

            var xml = doc.ToString();
            return xml;
        }
    }
}
