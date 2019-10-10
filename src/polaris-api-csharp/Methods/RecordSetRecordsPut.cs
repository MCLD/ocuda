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
        public PapiResponse<PAPIResult> RecordSetRecordsPut(int recordSetId, IEnumerable<int> records, RecordSetPutActions action, int userId = 1, int workstationId = 1)
        {
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/recordsets/{recordSetId}?action={action.ToString()}&userid={userId}&wsid={workstationId}";
            var body = $"<?xml version=\"1.0\"?><ModifyRecordSetContent><Records>{string.Join(",", records)}</Records></ModifyRecordSetContent>";
            return Execute<PAPIResult>(HttpMethod.Put, url, Token.AccessSecret, body);
        }

        public PapiResponse<PAPIResult> RecordSetRecordsAdd(int recordSetId, int recordId, int userId = 1, int workstationId = 1)
        {
            return RecordSetRecordsPut(recordSetId, new[] { recordId }, RecordSetPutActions.Add, userId, workstationId);
        }


        public PapiResponse<PAPIResult> RecordSetRecordsAdd(int recordSetId, IEnumerable<int> records, int userId = 1, int workstationId = 1)
        {
            return RecordSetRecordsPut(recordSetId, records, RecordSetPutActions.Add, userId, workstationId);
        }

        public PapiResponse<PAPIResult> RecordSetRecordsRemove(int recordSetId, int recordId, int userId = 1, int workstationId = 1)
        {
            return RecordSetRecordsPut(recordSetId, new[] { recordId }, RecordSetPutActions.Remove, userId, workstationId);
        }


        public PapiResponse<PAPIResult> RecordSetRecordsRemove(int recordSetId, IEnumerable<int> records, int userId = 1, int workstationId = 1)
        {
            return RecordSetRecordsPut(recordSetId, records, RecordSetPutActions.Remove, userId, workstationId);
        }
    }

    public enum RecordSetPutActions
    {
        Remove = 0,
        Add
    }
}
