using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.EngineWrapper.SortingEngine;
using System.Buffers;

namespace test.TestNonGenericsSeries
{
    public class TestSeriesUtilities
    {
        [Fact]
        public void TestSortingValue()
        {
            List<int> data = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                data.Add(rnd.Next());
            }

            var series = new Series(data);
            data.Sort();
            series = series.SortValues();

            // test
            for (int i = 0; i < 100; i++)
            {
                Assert.True(series.Values.Select(v => Convert.ToInt32(v)).ToList().SequenceEqual(data));
            }
        }

        [Fact]
        public void TestSortingRangeIndex()
        {
            List<int> data = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                data.Add(rnd.Next());
            }

            var series = new Series(data);
            series = series.SortIndex();

            // test
            // this series used range index so the order of the value is unchanged
            for (int i = 0; i < 100; i++)
            {
                Assert.True(series.Values.Select(v => Convert.ToInt32(v)).ToList().SequenceEqual(data));
            }
        }

        [Fact]
        public void SortIndex_ShouldSortSeriesValuesInSameOrderAsEngine()
        {
            // ---------- Arrange ----------
            var random = new Random();

            // Create random data values
            var values = Enumerable.Range(0, 100)
                                   .Select(_ => random.Next())
                                   .ToList();

            // Create index from 1 to 100, then shuffle
            var customIndex = Enumerable.Range(1, 100)
                                        .OrderBy(_ => random.Next())
                                        .Select(i => (object)i)
                                        .ToList();

            // Clone values array for engine-based sorting
            var expectedValues = values.ToArray();

            // ---------- Act ----------
            var series = new Series(values, customIndex);
            series = series.SortIndex();

            IndexValueSorter.SortByIndex(expectedValues, customIndex, true);

            // ---------- Assert ----------
            var sortedValues = series.Values.Select(v => Convert.ToInt32(v)).ToList();
            Assert.True(sortedValues.SequenceEqual(expectedValues),
                        "Series values should match engine-sorted values after SortIndex()");
        }

        [Fact]
        public void SortIndex_ShouldSortSeriesValues_WhenIndexIsDescending()
        {
            // ---------- Arrange ----------
            var random = new Random();

            // Create random data values
            var values = Enumerable.Range(0, 100)
                                   .Select(_ => random.Next())
                                   .ToList();

            // Create index from 100 down to 1
            var customIndex = Enumerable.Range(1, 100)
                                        .Reverse()
                                        .Select(i => (object)i)
                                        .ToList();

            // Clone values for engine-based sorting
            var expectedValues = values.ToArray();

            // ---------- Act ----------
            var series = new Series(values, customIndex);
            series = series.SortIndex();

            IndexValueSorter.SortByIndex(expectedValues, customIndex);

            // ---------- Assert ----------
            var sortedValues = series.Values.Select(v => Convert.ToInt32(v)).ToList();
            Assert.True(sortedValues.SequenceEqual(expectedValues),
                        "Series values should match engine-sorted values when index is in descending order.");
        }

        [Fact]
        public void SortIndex_ShouldSortSeriesValues_WhenIndexIsString()
        {
            // ---------- Arrange ----------
            var random = new Random();

            // Create random data values
            var values = Enumerable.Range(0, 100)
                                   .Select(_ => random.Next())
                                   .ToList();

            // Create string index from "Item1" to "Item100", then shuffle
            var customIndex = Enumerable.Range(1, 100)
                                        .Select(i => $"Item{i}")
                                        .OrderBy(_ => random.Next())
                                        .Cast<object>()
                                        .ToList();

            // Clone values for engine-based sorting
            var expectedValues = values.ToArray();

            // ---------- Act ----------
            var series = new Series(values, customIndex);
            series = series.SortIndex();

            IndexValueSorter.SortByIndex(expectedValues, customIndex);

            // ---------- Assert ----------
            var sortedValues = series.Values.Select(v => Convert.ToInt32(v)).ToList();
            Assert.True(sortedValues.SequenceEqual(expectedValues),
                        "Series values should match engine-sorted values when index is string.");
        }

        [Fact]
        public void SortIndex_ShouldSortSeriesValues_WhenIndexIsUnicodeString()
        {
            // ---------- Arrange ----------
            var random = new Random();

            var values = Enumerable.Range(0, 20)
                                   .Select(_ => random.Next())
                                   .ToList();

            var unicodeIndexBase = new[]
            {
                "áo", "bàn", "cá", "đèn", "ếch",
                "龍", "虎", "馬", "山", "水",
                "😀", "🚀", "🌸", "🍕", "🎉"
            };

            var unicodeIndex = unicodeIndexBase
                .Concat(Enumerable.Range(0, values.Count - unicodeIndexBase.Length)
                                  .Select(i => $"ký{i}"))
                .OrderBy(_ => random.Next())
                .Cast<object>()
                .ToList();

            var expectedValues = values.ToArray();

            // Custom comparer for Unicode (ordinal)
            var comparer = Comparer<object?>.Create((x, y) =>
            {
                return StringComparer.Ordinal.Compare(x?.ToString(), y?.ToString());
            });

            // ---------- Act ----------
            var series = new Series(values, unicodeIndex);
            series = series.SortIndex(comparer);

            IndexValueSorter.SortByIndexWithComparer(expectedValues, unicodeIndex, comparer);

            // ---------- Assert ----------
            var sortedValues = series.Values.Select(v => Convert.ToInt32(v)).ToList();
            Assert.True(sortedValues.SequenceEqual(expectedValues),
                        "Series values should match engine-sorted values for Unicode index.");
        }
    }
}
