using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.DataProvider.SqlServer.Promenade
{
    public class Context : Data.PromenadeContext
    {
        public Context(DbContextOptions options) : base(options) { }
    }
}
