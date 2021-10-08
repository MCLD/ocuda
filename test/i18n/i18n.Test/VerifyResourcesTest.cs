using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ocuda.i18n.Keys;
using Ocuda.Utility.Helpers;
using Xunit;

namespace i18n.Test
{
    /// <summary>
    /// Verify that all the i18n text resources are present in both the resource files for English
    /// and Spanish.
    /// </summary>
    public class VerifyResourcesTest
    {
        private const string BasePathToResx = "..{0}..{0}..{0}..{0}..{0}..{0}src{0}i18n{0}Resources";
        private const char NonBreakingSpaceCharacter = (char)160;
        private const char SpaceCharacter = (char)32;
        private readonly string PathToResx;

        public VerifyResourcesTest()
        {
            PathToResx = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory,
                string.Format(BasePathToResx, Path.DirectorySeparatorChar)));
        }

        [Fact]
        public void TestEnglishResxHasAllItems() => TestResxHasItems("Shared.en.resx");

        [Fact]
        public void TestSpanishResxHasAllItems() => TestResxHasItems("Shared.es.resx");

        private void TestResxHasItems(string filename)
        {
            var resourceFileKeys = XmlHelper.ExtractDataNames(filename, PathToResx)
                .Keys
                .Select(_ => _.Replace(NonBreakingSpaceCharacter, SpaceCharacter));

            var constStrings = typeof(Promenade).GetFields(BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.FlattenHierarchy)
                .Where(_ => _.FieldType == typeof(string));

            var constValues = constStrings.Select(_ => (string)_.GetValue(null));

            Assert.Empty(resourceFileKeys.Except(constValues));
        }
    }
}