using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.DataProvider.SQLite.Promenade
{
    public class Context : Data.PromenadeContext
    {
        public Context(DbContextOptions options) : base(options) { }

    }
}