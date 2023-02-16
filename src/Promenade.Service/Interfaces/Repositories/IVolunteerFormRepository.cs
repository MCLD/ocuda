using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IVolunteerFormRepository : IGenericRepository<VolunteerForm>
    {
        Task<ICollection<VolunteerForm>> FindAllAsync();

        Task<VolunteerForm> FindByTypeAsync(VolunteerFormType type);
    }
}