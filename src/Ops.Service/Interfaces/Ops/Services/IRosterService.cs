using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IRosterService
    {
        Task<int> UploadRosterAsync(int currentUserId, string filename);
        Task<(RosterHeader RosterDetail,
              IEnumerable<RosterDetail> NewEmployees,
              IEnumerable<RosterDetail> RemovedEmployees)> GetRosterChangesAsync();
        Task<RosterDetail> GetLatestDetailsAsync(string email);
        Task<bool> ApproveRosterChanges(int rosterEntryId);
    }
}
