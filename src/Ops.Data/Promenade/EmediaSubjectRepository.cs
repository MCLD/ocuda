using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaSubjectRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaSubjectRepository> logger)
            : GenericRepository<PromenadeContext, EmediaSubject>(repositoryFacade, logger),
            IEmediaSubjectRepository
    {
        public async Task<ICollection<EmediaSubject>> GetAllForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.EmediaId == emediaId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaSubject>> GetAllForGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.Emedia.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaSubject>> GetBySubjectIdAsync(int subjectId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<ICollection<string>> GetEmediasForSubjectAsync(int subjectId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.SubjectId == subjectId)
                .Select(_ => _.Emedia.Name)
                .OrderBy(_ => _)
                .ToListAsync();
        }

        public async Task<ICollection<int>> GetSubjectIdsForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.EmediaId == emediaId)
                .Select(_ => _.SubjectId)
                .ToListAsync();
        }

        public async Task<ICollection<Subject>> GetSubjectsForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.EmediaId == emediaId)
                .Select(_ => _.Subject)
                .ToListAsync();
        }

        public void RemoveByEmediaAndSubjects(int emediaId, ICollection<int> subjectIds)
        {
            var emediaSubjects = DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive
                    && _.EmediaId == emediaId
                    && subjectIds.Contains(_.SubjectId));

            DbSet.RemoveRange(emediaSubjects);
        }
    }
}