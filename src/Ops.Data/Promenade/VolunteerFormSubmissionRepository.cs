using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class VolunteerFormSubmissionRepository
        : GenericRepository<PromenadeContext, VolunteerFormSubmission>,
        IVolunteerFormSubmissionRepository
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public VolunteerFormSubmissionRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormSubmissionRepository> logger,
            IDateTimeProvider dateTimeProvider) : base(repositoryFacade, logger)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);

            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ICollection<VolunteerFormSubmission>> GetAllAsync(int locationId, int formId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId && _.VolunteerFormId == formId)
                .OrderByDescending(_ => _.CreatedAt)
                .ToListAsync();
        }

        public async Task<VolunteerFormSubmission> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Return a paginated list of selected volunteer form submission fields (CreatedAt, Id,
        /// LocationId, Name, StaffNotifiedAt, VolunteerFormType)
        /// </summary>
        /// <param name="filter">A VolunteerSubmissionFilter for pagination and form type
        /// filtering</param>
        /// <returns>A collection of VolunteerFormSubmission objects and a count of the total
        /// matching the filter</returns>
        /// <exception cref="ArgumentNullException">Thrown if the filter is null.</exception>
        public async Task<CollectionWithCount<VolunteerFormSubmission>> GetPaginatedListAsync(VolunteerSubmissionFilter filter)
        {
            if (filter == null) { throw new ArgumentNullException(nameof(filter)); }

            if (!filter.Take.HasValue || filter.Take > 30)
            {
                filter.Take = 30;
            }

            var formToTypeMapping = await _context
                .VolunteerForms
                .AsNoTracking()
                .ToDictionaryAsync(k => k.Id, v => v.VolunteerFormType);

            var query = filter.FormType.HasValue
                ? DbSet.Where(_ => _.VolunteerForm.VolunteerFormType == filter.FormType)
                : DbSet;

            return new CollectionWithCount<VolunteerFormSubmission>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .Select(_ => new VolunteerFormSubmission
                    {
                        CreatedAt = _.CreatedAt,
                        Id = _.Id,
                        LocationId = _.LocationId,
                        Name = _.Name,
                        StaffNotifiedAt = _.StaffNotifiedAt,
                        VolunteerFormType = formToTypeMapping.ContainsKey(_.VolunteerFormId)
                            ? formToTypeMapping[_.VolunteerFormId]
                            : null
                    })
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<ICollection<VolunteerFormSubmission>> GetPendingNotificationsAsync()
        {
            return await DbSet
                .Where(_ => !_.StaffNotifiedAt.HasValue)
                .Select(_ => new VolunteerFormSubmission
                {
                    Id = _.Id,
                    LocationId = _.LocationId,
                    CreatedAt = _.CreatedAt,
                    VolunteerFormId = _.VolunteerFormId,
                    VolunteerFormType = _.VolunteerForm.VolunteerFormType
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task StaffNotifiedAsync(int submissionId)
        {
            var submission = await DbSet.FindAsync(submissionId);
            if (submission?.StaffNotifiedAt.HasValue == false)
            {
                submission.StaffNotifiedAt = _dateTimeProvider.Now;
                DbSet.Update(submission);
                await SaveAsync();
            }
        }
    }
}