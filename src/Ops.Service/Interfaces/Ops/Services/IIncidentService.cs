using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IIncidentService
    {
        public Task<int> AddAsync(Incident incident,
            ICollection<IncidentStaff> staffs,
            ICollection<IncidentParticipant> participants,
            System.Uri baseUri);

        public Task AddFollowupAsync(int incidentId, string followupText);

        public Task AddRelationshipAsync(int incidentId, int relatedIncidentId);

        public Task AddTypeAsync(string incidentTypeName);

        public Task AdjustTypeStatusAsync(int incidentTypeId, bool status);

        public Task<IDictionary<int, string>> GetActiveIncidentTypesAsync();

        public Task<Dictionary<int, string>> GetAllIncidentTypesAsync();

        public Task<Incident> GetAsync(int incidentId);

        public Task<CollectionWithCount<IncidentType>> GetIncidentTypesAsync(BaseFilter filter);

        public Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter);

        public Task<IncidentType> GetTypeAsync(string incidentTypeDescription);

        public Task UpdateIncidentTypeAsync(int incidentTypeId, string incidentTypeDescription);
    }
}
