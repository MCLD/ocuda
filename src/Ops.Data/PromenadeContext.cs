using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        public PromenadeContext(DbContextOptions options) : base(options) { }
    }
}
