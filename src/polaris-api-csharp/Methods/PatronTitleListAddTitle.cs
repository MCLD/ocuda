using Clc.Polaris.Api.Helpers;
using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api
{
    public partial class PapiClient
    {
        public PapiResponse<PatronTitleListAddTitleResult> PatronTitleListAddTitle(string barcode, string password, int recordStoreId, string recordName, int localControlNumber)
        {
            //public/{version}/{lang-ID}/{app-ID}/{org-ID}/patron/{patron_barcode}/patrontitlelistaddtitle
            var url = $"/PAPIService/REST/public/v1/1033/100/1/patron/{barcode}/patrontitlelistaddtitle/";
            var xml = PatronTitleListHelper.BuildAddTitleXml(recordStoreId, recordName, localControlNumber);
            var response = Execute<PatronTitleListAddTitleResult>(HttpMethod.Post, url, password, xml);
            return response;
        }
    }
}
