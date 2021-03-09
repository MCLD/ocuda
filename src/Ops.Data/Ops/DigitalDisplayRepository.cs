using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Ops
{
    public class DigitalDisplayRepository
        : OpsRepository<OpsContext, DigitalDisplay, int>, IDigitalDisplayRepository
    {
        public DigitalDisplayRepository(Repository<OpsContext> repositoryFacade,
            ILogger<DigitalDisplayRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<DigitalDisplay>> GetAllAsync()
        {
            return await DbSet
                .OrderBy(_ => _.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateLastAttemptAsync(int displayId, DateTime lastAttemptAt)
        {
            var display = DbSet.SingleOrDefault(_ => _.Id == displayId);
            if (display != null)
            {
                display.LastAttempt = lastAttemptAt;
                Update(display);
                await SaveAsync();
            }
            else
            {
                throw new OcudaException("Unable to find display id {displayId}");
            }
        }

        public async Task UpdateLastCommunicationAsync(int displayId, DateTime lastCommunicationAt)
        {
            var display = DbSet.SingleOrDefault(_ => _.Id == displayId);
            if (display != null)
            {
                display.LastCommunication = lastCommunicationAt;
                Update(display);
                await SaveAsync();
            }
            else
            {
                throw new OcudaException("Unable to find display id {displayId}");
            }
        }

        public async Task UpdateLastVerificationAsync(int displayId, DateTime lastVerifiedAt)
        {
            var display = DbSet.SingleOrDefault(_ => _.Id == displayId);
            if (display != null)
            {
                display.LastContentVerification = lastVerifiedAt;
                display.LastCommunication = lastVerifiedAt;
                Update(display);
                await SaveAsync();
            }
            else
            {
                throw new OcudaException("Unable to find display id {displayId}");
            }
        }
    }
}