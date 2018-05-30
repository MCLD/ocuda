using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.DataProvider.SQLite.Ops
{
    public class Context : Data.OpsContext
    {
        public Context(DbContextOptions options) : base(options) { }

    }
}
