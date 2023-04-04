using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Models
{
    public class StatusReport
    {
        public StatusReport()
        {
            Status = new Dictionary<string, ICollection<StatusMessage>>();
            StatusCounts = new Dictionary<string, int>();
        }

        [JsonConstructor]
        public StatusReport(DateTime asOf,
            IDictionary<string, ICollection<StatusMessage>> status,
            IDictionary<string, int> statusCounts)
            => (AsOf, Status, StatusCounts) = (asOf, status, statusCounts);

        public DateTime AsOf { get; set; }

        public IDictionary<string, ICollection<StatusMessage>> Status { get; }
        public IDictionary<string, int> StatusCounts { get; }

        public void AddStatus(string key, string addValue)
        {
            AddStatus(key, addValue, LogLevel.Debug);
        }

        public void AddStatus(string key, string message, LogLevel status)
        {
            if (Status.TryGetValue(key, out ICollection<StatusMessage> value))
            {
                value.Add(new StatusMessage
                {
                    Message = message,
                    Status = status
                });
            }
            else
            {
                Status.Add(key, new List<StatusMessage>
                {
                    new StatusMessage
                    {
                        Message = message,
                        Status = status
                    }
                });
            }
        }
    }
}