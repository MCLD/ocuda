using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }
    }
}
