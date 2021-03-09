using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplayRepository : IOpsRepository<DigitalDisplay, int>
    {
        public Task<ICollection<DigitalDisplay>> GetAllAsync();

        public Task UpdateLastAttemptAsync(int displayId, DateTime lastAttemptAt);

        public Task UpdateLastCommunicationAsync(int displayId, DateTime lastCommunicationAt);

        public Task UpdateLastVerificationAsync(int displayId, DateTime lastVerifiedAt);
    }
}