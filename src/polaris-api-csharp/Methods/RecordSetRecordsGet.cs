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
        public PapiResponse<RecordSetRecordsGetResult> RecordSetRecordsGet(int recordSetId, int userId = 1, int workstationId = 1, int startIndex = 0, int numRecords = 1000)
        {
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/recordsets/{recordSetId}/records?startIndex={startIndex}&numRecords={numRecords}&userid={userId}&wsid={workstationId}";
            return Execute<RecordSetRecordsGetResult>(HttpMethod.Get, url, Token.AccessSecret);
        }
    }
}
