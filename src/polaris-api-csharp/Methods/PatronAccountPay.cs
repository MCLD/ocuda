using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;


namespace Clc.Polaris.Api
{
    public partial class PapiClient
    {
        /// <summary>
        /// Pay a patron fine or fee
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="txnId"></param>
        /// <param name="txnAmount"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="wsid"></param>
        /// <param name="userid"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public PapiResponse<PatronAccountPayResult> PatronAccountPay(string barcode, int txnId, double txnAmount, PaymentMethod paymentMethod, int wsid = 1, int userid = 1, string note = "")
        {
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/patron/{barcode}/account/{txnId}/pay?wsid={wsid}&userid={userid}";

            var doc = new XDocument(
                new XElement("PatronAccountPayData",
                         new XElement("TxnAmount", txnAmount),
                         new XElement("PaymentMethodID", (int)paymentMethod),
                         new XElement("FreeTextNote", note)
                )
            );

            var response = Execute<PatronAccountPayResult>(HttpMethod.Put, url, Token.AccessSecret, doc.ToString());
            return response;
        }
    }
}
