using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops
{
    public class Context : Data.OpsContext
    {
        public Context(DbContextOptions<Context> options) 
            : base(options) { }
    }
}
