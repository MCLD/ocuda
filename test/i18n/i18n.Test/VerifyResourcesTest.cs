using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ocuda.i18n.l10nKeys;
using Ocuda.Utility.Helpers;
using Xunit;

namespace i18n.Test
{
    public class VerifyResourcesTest
    {
        private const string BasePathToResx = "..{0}..{0}..{0}..{0}..{0}..{0}src{0}i18n{0}Resources";

        private readonly string PathToResx;

        public VerifyResourcesTest()
        {
            PathToResx = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory,
                string.Format(BasePathToResx, Path.DirectorySeparatorChar)));
        }

        [Fact]
        public void TestEnglishResxHasAllAnnotations() => TestResxHasAnnotations("Shared.en.resx");

        [Fact]
        public void TestSpanishResxHasAllAnnotations() => TestResxHasAnnotations("Shared.es.resx");

        private void TestResxHasAnnotations(string filename)
        {
            var resourceFileKeys = XmlHelper.ExtractDataNames(filename, PathToResx).Keys;

            var annotationValues = new List<string>();
            foreach (var classType in typeof(Annotations).GetNestedTypes(BindingFlags.Public))
            {
                var constStrings = classType.GetFields(BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .ToList();

                foreach (var constval in constStrings)
                {
                    annotationValues.Add((string)constval.GetValue(null));
                }
            }

            var displayNameValues = new List<string>();
            foreach (var classType in typeof(DisplayNames).GetNestedTypes(BindingFlags.Public))
            {
                var constStrings = classType.GetFields(BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .ToList();

                foreach (var constval in constStrings)
                {
                    displayNameValues.Add((string)constval.GetValue(null));
                }
            }

            var errorMessageValues = new List<string>();
            foreach (var classType in typeof(ErrorMessages).GetNestedTypes(BindingFlags.Public))
            {
                var constStrings = classType.GetFields(BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .ToList();

                foreach (var constval in constStrings)
                {
                    errorMessageValues.Add((string)constval.GetValue(null));
                }
            }

            var localizationValues = annotationValues
                .Concat(displayNameValues)
                .Concat(errorMessageValues);

            var missingItems = localizationValues.Except(resourceFileKeys);

            Assert.Empty(missingItems);
        }
    }
}
