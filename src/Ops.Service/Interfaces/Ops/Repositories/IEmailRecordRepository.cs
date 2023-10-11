using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmailRecordRepository
    {
        public Task<EmailRecord> AddSaveAsync(EmailRecord emailRecord);
    }
}