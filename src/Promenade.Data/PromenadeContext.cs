using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        public PromenadeContext(DbContextOptions options) : base(options) { }
    }
}
