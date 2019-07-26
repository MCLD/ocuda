using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ops.Data.Test
{
    [TestClass]
    public class PromenadeMatch
    {
        [TestMethod]
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


            using (var opsProm = new Ocuda.Ops.DataProvider.SqlServer.Promenade.Context(opsPromOptions))
            {
                using (var promProm = new Ocuda.Promenade.DataProvider.SqlServer.Promenade.Context(promPromOptions))
                {
                    var opsPromDbSets = opsProm.GetType()
                        .GetProperties()
                        .Where(_ => _.PropertyType.IsGenericType
                            && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
                        .Select(_ => _.Name);

                    var promPromDbSets = promProm.GetType()
                        .GetProperties()
                        .Where(_ => _.PropertyType.IsGenericType
                            && typeof(DbSet<>).IsAssignableFrom(_.PropertyType.GetGenericTypeDefinition()))
                        .Select(_ => _.Name);

                    foreach (var dbSetName in opsPromDbSets.Except(promPromDbSets))
                    {
                        Assert.Fail("DbSet present in Ocuda.Ops Promenade Context but missing from Ocuda.Promenade Promenade Context: {0}", dbSetName);
                    }

                    foreach (var dbSetName in promPromDbSets.Except(opsPromDbSets))
                    {
                        Assert.Fail("DbSet present in Ocuda.Promenade Promenade Context but missing from Ocuda.Ops Promenade Context: {0}", dbSetName);
                    }
                }
            }
        }
    }
}
