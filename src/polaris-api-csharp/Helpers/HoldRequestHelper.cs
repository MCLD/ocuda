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
    /// Helper methods for PAPI hold request methods
    /// </summary>
    public class HoldRequestHelper
    {
        /// <summary>
        /// Build the XML for PAPI hold request activation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="activationDate"></param>
        /// <returns></returns>
        public static string BuildActivationXml(int userId, DateTime activationDate)
        {
            var doc = new XDocument(
                new XElement("HoldRequestActivationData",
                             new XElement("UserID", userId),
                             new XElement("ActivationDate", activationDate.ToString("yyyy-MM-dd"))
                    )
                );
            return doc.ToString();
        }

        /// <summary>
        /// Build the XML for PAPI hold request creation
        /// </summary>
        /// <param name="holdParams"></param>
        /// <returns></returns>
        public static string BuildHoldRequestCreateXml(HoldRequestCreateParams holdParams)
        {
            var doc = new XDocument();
            var root = new XElement("HoldRequestCreateData");

            root.Add(new XElement("PatronID", holdParams.PatronID));
            root.Add(new XElement("BibID", holdParams.BibID));
            root.Add(new XElement("ItemBarcode", holdParams.ItemBarcode ?? ""));
            root.Add(new XElement("VolumeNumber", holdParams.VolumeNumber ?? ""));
            root.Add(new XElement("Designation", holdParams.Designation ?? ""));
            root.Add(new XElement("PickupOrgID", holdParams.PickupOrgID));
            root.Add(new XElement("IsBorrowByMail", holdParams.IsBorrowByMail));
            root.Add(new XElement("PatronNotes", holdParams.PatronNotes));
            root.Add(new XElement("ActivationDate", holdParams.ActivationDate.ToString("yyyy-MM-dd")));
            root.Add(new XElement("WorkstationID", holdParams.WorkstationID));
            root.Add(new XElement("UserID", holdParams.UserID));
            root.Add(new XElement("RequestingOrgID", holdParams.RequestingOrgID));
            root.Add(new XElement("TargetGUID", holdParams.TargetGUID ?? null));

            doc.Add(root);

            return doc.ToString();
        }

        /// <summary>
        /// Build the XML to reply to a PAPI hold request creation
        /// </summary>
        /// <param name="holdRequest"></param>
        /// <param name="requestingOrgId"></param>
        /// <param name="answer">See PAPI documentation</param>
        /// <param name="state">See PAPI documentation</param>
        /// <returns></returns>
        public static string BuildHoldRequestReplyXml(HoldRequestResult holdRequest, int requestingOrgId, int answer, int state)
        {
            var doc = new XDocument(
                new XElement("HoldRequestReplyData",
                             new XElement("TxnGroupQualifier", holdRequest.TxnGroupQualifier),
                             new XElement("TxnQualifier", holdRequest.TxnQualifier),
                             new XElement("RequestingOrgID", requestingOrgId),
                             new XElement("Answer", answer),
                             new XElement("State", state)
                    )
                );

            return doc.ToString();
        }
    }
}
