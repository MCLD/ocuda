using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase, IMigratableContext
    {
        public PromenadeContext(DbContextOptions options) : base(options) { }

        #region IMigratableContext
        public new void Migrate() => Database.Migrate();
        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();
        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();
        #endregion IMigratableContext
    }
}
