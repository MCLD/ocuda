using System;
using Microsoft.Extensions.Configuration;
using Ocuda.Utility.Data;

namespace Ocuda.Ops.Data.ServiceFacade
{
    public class Repository<TContext>
        where TContext : DbContextBase
    {
        public readonly TContext context;
        public readonly IConfiguration config;

        public Repository(TContext context,
            IConfiguration config)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}