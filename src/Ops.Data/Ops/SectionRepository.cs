using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionRepository : GenericRepository<OpsContext, Section, int>, ISectionRepository
    {
        public SectionRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SectionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public Section GetSectionByStub(string stub)
        {
            return DbSet
            .AsNoTracking()
            .Where(_ => _.Stub == stub)
            .FirstOrDefault();
        }

        public Section GetSectionByName(string name)
        {
            return DbSet
            .AsNoTracking()
            .Where(_ => _.Name == name)
            .FirstOrDefault();
        }

        public List<Section> GetAllSections()
        {
            return DbSet
            .AsNoTracking()
            .ToList();
        }
    }
}
