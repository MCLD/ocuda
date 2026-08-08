using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IJobConfigurationRepository : IGenericRepository<JobConfiguration>
    {
        /// <summary>
        /// Get all available configured job types.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of JobTypes that are configured.</returns>
        public Task<IEnumerable<JobType>> GetAllTypes();

        /// <summary>
        /// Get job configurations which are eligible for automatic scheduling.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of JobConfigurations.</returns>
        public Task<IEnumerable<JobConfiguration>> GetAutomaticallyScheduleAsync();
    }
}
