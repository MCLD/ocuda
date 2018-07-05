using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionManagerGroupRepository
        : GenericRepository<Models.SectionManagerGroup, int>, ISectionManagerGroupRepository
    {
        public SectionManagerGroupRepository(OpsContext context, 
            ILogger<SectionManagerGroupRepository> logger)
            : base(context, logger)
        {
        }
    }
}
