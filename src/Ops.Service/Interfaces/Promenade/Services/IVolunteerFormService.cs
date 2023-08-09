using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IVolunteerFormService
    {
        Task AddFormUserMapping(int locationId, VolunteerFormType type, int userId);

        Task<VolunteerForm> AddUpdateFormAsync(VolunteerForm form);

        Task AddVolunteerLocationFeature(int featureId, int locationId, string locationStub);

        Task DisableAsync(int formId);

        Task<VolunteerForm> EditAsync(VolunteerForm form);

        Task EnableAsync(int formId);

        IDictionary<string, int> GetAllVolunteerFormTypes();

        Task<VolunteerForm> GetFormByIdAsync(int Id);

        Task<ICollection<VolunteerForm>> GetFormBySegmentIdAsync(int segmentId);

        Task<VolunteerForm> GetFormByTypeAsync(VolunteerFormType type);

        Task<IDictionary<VolunteerFormType, int>> GetFormEmailSetupMappingAsync();

        Task<ICollection<VolunteerFormUserMapping>>
            GetFormUserMappingsAsync(int formId, int locationId);

        Task<ICollection<VolunteerFormSubmissionEmailRecord>>
            GetNotificationInfoAsync(int submissionId);

        Task<DataWithCount<ICollection<VolunteerForm>>> GetPaginatedListAsync(BaseFilter filter);

        Task<CollectionWithCount<VolunteerFormSubmission>>
            GetPaginatedSubmissionsAsync(VolunteerSubmissionFilter filter);

        Task<ICollection<VolunteerFormSubmission>> GetPendingNotificationsAsync();

        Task<ICollection<VolunteerFormUserMapping>> GetUserMappingsAsync(int userId);

        Task<ICollection<VolunteerForm>> GetVolunteerFormsAsync();

        Task<VolunteerFormSubmission> GetVolunteerFormSubmissionAsync(int submissionId);

        Task<ICollection<VolunteerFormSubmission>>
            GetVolunteerFormSubmissionsAsync(int locationId, int typeId);

        Task NotifiedStaffAsync(int formSubmissionId, int emailRecordId, int userId);

        Task RemoveFormUserMapping(int locationId, int userId, VolunteerFormType type);
    }
}