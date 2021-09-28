using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Ops.Data.Test
{
    public class ContextFixture : IDisposable
    {
        public ContextFixture()
        {
            var opsPromOptions
                = new DbContextOptionsBuilder<Ocuda.Ops.DataProvider.SqlServer.Promenade.Context>()
                .UseInMemoryDatabase(databaseName: "Promenade")
                .Options;

            var promPromOptions
                = new DbContextOptionsBuilder<Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context>()
                .UseInMemoryDatabase(databaseName: "Promenade")
                .Options;

            OpsProm = new Ocuda.Ops.DataProvider.SqlServer.Promenade.Context(opsPromOptions);
            PromProm = new Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context(promPromOptions);

            OpsPromDbSets = OpsProm.GetType()
               .GetProperties()
               .Where(_ => _.PropertyType.IsGenericType
               && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
               .Select(_ => _.Name);

            PromPromDbSets = PromProm.GetType()
               .GetProperties()
               .Where(_ => _.PropertyType.IsGenericType
                   && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
               .Select(_ => _.Name);
        }

        public Ocuda.Ops.DataProvider.SqlServer.Promenade.Context OpsProm { get; set; }
        public IEnumerable<string> OpsPromDbSets { get; set; }
        public Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context PromProm { get; set; }
        public IEnumerable<string> PromPromDbSets { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OpsProm?.Dispose();
                OpsProm = null;

                PromProm?.Dispose();
                PromProm = null;
            }
        }
    }
}