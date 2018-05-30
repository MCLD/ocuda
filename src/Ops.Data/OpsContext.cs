using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.Data
{
    public abstract class OpsContext : Utility.Data.DbContextBase
    {
        public OpsContext(DbContextOptions options) : base(options) { }

        public DbSet<Models.SiteSetting> SiteSettings { get; set; }
    }
}
