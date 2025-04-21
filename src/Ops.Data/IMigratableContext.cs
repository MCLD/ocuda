using System.Collections.Generic;

namespace Ocuda.Ops.Data
{
    public interface IMigratableContext
    {
        void Migrate();
        IEnumerable<string> GetPendingMigrationList();
        string GetCurrentMigration();
    }
}
