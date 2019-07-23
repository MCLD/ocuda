using System;
using Microsoft.Extensions.Configuration;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Data;

namespace Ocuda.Promenade.Data.ServiceFacade
{
    public class Repository<TContext>
        where TContext : DbContextBase
    {
        public readonly TContext context;
        public readonly IConfiguration config;
        public readonly IDateTimeProvider dateTimeProvider;

        public Repository(TContext context,
            IConfiguration config,
            IDateTimeProvider dateTimeProvider)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.dateTimeProvider = dateTimeProvider 
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }
    }
}
