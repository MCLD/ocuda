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

        [Fact]
        public void OpsPromMissingPromProm()
            => Assert.Empty(_fixture.OpsPromDbSets.Except(_fixture.PromPromDbSets));

        [Fact]
        public void PromPromMissingOpsProm()
            => Assert.Empty(_fixture.PromPromDbSets.Except(_fixture.OpsPromDbSets));
    }
}