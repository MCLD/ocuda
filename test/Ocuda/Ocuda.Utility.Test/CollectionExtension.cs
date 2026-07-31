using Ocuda.Utility.Extensions;

namespace Ocuda.Utility.Test
{
    public class CollectionExtension
    {
        [Fact]
        public void VerifyAddRange()
        {
            string[] items1 = ["a", "b", "c"];
            string[] items2 = ["d", "e", "f"];

            ICollection<string> addRangeList = [.. items1];
            addRangeList.AddRange(items2);

            var union = items1.Union(items2).ToList();

            Assert.Equal(union.Count, addRangeList.Count);

            foreach (var item in union)
            {
                Assert.Contains(item, addRangeList);
            }
            foreach (var item in addRangeList)
            {
                Assert.Contains(item, union);
            }
        }

        [Fact]
        public void VerifyHumanCommaList()
        {
            string[] items1 = ["a"];
            string[] items2 = ["b", "c"];
            string[] items3 = ["d", "e", "f"];

            Assert.Equal(items1.HumanCommaList(), items1[0]);
            Assert.Equal(items2.HumanCommaList(), $"{items2[0]}, and {items2[1]}");
            Assert.Equal(items3.HumanCommaList(), $"{items3[0]}, {items3[1]}, and {items3[2]}");
        }
    }
}
