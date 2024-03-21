using System.Linq;
using Xunit;

namespace Ops.Data.Test
{
    public class PromenadeMatch : IClassFixture<ContextFixture>
    {
        private readonly ContextFixture _fixture;

        public PromenadeMatch(ContextFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Verify context items in Ops and Prom contexts - if this test fails, listed
        /// properties are present in Ocuda.Ops.Data.PromenadeContext and are missing
        /// in Ocuda.Promenade.Data.PromenadeContext
        /// </summary>
        [Fact]
        public void InOpsDataMissingPromData()
            => Assert.Empty(_fixture.OpsPromDbSets.Except(_fixture.PromPromDbSets));

        /// <summary>
        /// Verify context items in Ops and Prom contexts - if this test fails, listed
        /// properties are present in Ocuda.Promenade.Data.PromenadeContext and are
        /// missing in Ocuda.Ops.Data.PromenadeContext
        /// </summary>
        [Fact]
        public void InPromDataMissingOpsData()
            => Assert.Empty(_fixture.PromPromDbSets.Except(_fixture.OpsPromDbSets));
    }
}