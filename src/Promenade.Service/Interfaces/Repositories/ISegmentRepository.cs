using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentRepository : IGenericRepository<Segment>
    {
        Task<Segment> FindAsync(int id);
        Task<Segment> GetActiveAsync(int id);
    }
}
