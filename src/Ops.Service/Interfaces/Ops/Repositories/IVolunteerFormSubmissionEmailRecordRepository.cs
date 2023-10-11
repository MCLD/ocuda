using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IVolunteerFormSubmissionEmailRecordRepository
    {
        public Task AddSaveAsync(int formSubmissionId, int emailRecordId, int userId);

        public Task<ICollection<VolunteerFormSubmissionEmailRecord>>
                    GetBySubmissionId(int submissionId);
    }
}