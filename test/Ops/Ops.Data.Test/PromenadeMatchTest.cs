using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ops.Data.Test
{
    public class PromenadeMatchTest
    {
        /// <summary>
        /// Verify that the Ocuda.Ops.DataProvider.SqlServer.Promenade.Context and
        /// Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context objects contain the same
        /// database elements so that site management (in Ops) and the site itself (in
        /// Promenade) match.
        /// </summary>
        [Fact]
        public void VerifyMatch()
        {
            var opsPromOptions
                = new DbContextOptionsBuilder<Ocuda.Ops.DataProvider.SqlServer.Promenade.Context>()
                .UseInMemoryDatabase(databaseName: "Promenade")
                .Options;

            var promPromOptions
                = new DbContextOptionsBuilder<Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context>()
                .UseInMemoryDatabase(databaseName: "Promenade")
                .Options;

            using var opsProm = new Ocuda.Ops.DataProvider.SqlServer.Promenade.Context(opsPromOptions);
            using var promProm = new Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context(promPromOptions);

            var opsPromDbSets = opsProm.GetType()
                .GetProperties()
                .Where(_ => _.PropertyType.IsGenericType
                && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
                .Select(_ => _.Name);

            Assert.NotEmpty(opsPromDbSets);

            var promPromDbSets = promProm.GetType()
                .GetProperties()
                .Where(_ => _.PropertyType.IsGenericType
                    && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
                .Select(_ => _.Name);

            Assert.NotEmpty(promPromDbSets);

            Assert.Empty(opsPromDbSets.Except(promPromDbSets));

            Assert.Empty(promPromDbSets.Except(opsPromDbSets));
        }
    }
}