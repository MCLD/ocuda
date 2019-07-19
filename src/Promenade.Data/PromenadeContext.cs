using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; }
    }
}
