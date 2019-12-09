using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade
{
    public class Context : Data.PromenadeContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options) { }
    }
}
