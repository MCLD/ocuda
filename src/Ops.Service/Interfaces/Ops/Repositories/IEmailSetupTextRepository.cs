using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmailSetupTextRepository : IGenericRepository<EmailSetupText>
    {
        public Task<EmailSetupText> GetByIdLanguageAsync(int emailSetupId, string languageName);
    }
}
