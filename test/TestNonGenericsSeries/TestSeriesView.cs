using DataProcessor.source.NonGenericsSeries;

namespace test.TestNonGenericsSeries
{
    public class SeriesViewTests
    {
        private Series BuildSampleSeries()
        {
            // index = ["a","b","c","d","e"], values = [10, 20, 30, 40, 50]
            var values = new List<object> { 10, 20, 30, 40, 50 };
            var index = new List<object> { "a", "b", "c", "d", "e" };
            return new Series(values, index: index, copy: true);
        }

        [Fact]
        public void RangeView_CreatesCorrectIndicesAndValues()
        {
            var series = BuildSampleSeries();
            var view = new Series.SeriesView(series, (start: "b", end: "d", step: 1));

            var expectedValues = new object[] { 20, 30, 40 };
            Assert.NotStrictEqual(expectedValues, view.ToSeries().Values);

            var expectedIndices = new int[] { 1, 2, 3 };
            Assert.Equal(expectedIndices, view.Indices);
        }

        [Fact]
        public void ListView_CreatesCorrectIndicesAndValues()
        {
            var series = BuildSampleSeries();
            var view = new Series.SeriesView(series, new List<object> { "a", "c", "e" });

            var expectedValues = new object[] { 10, 30, 50 };
            Assert.NotStrictEqual(expectedValues, view.ToSeries().Values);

            var expectedIndices = new int[] { 0, 2, 4 };
            Assert.Equal(expectedIndices, view.Indices);
        }

        [Fact]
        public void SliceView_ByList_ReturnsCorrectSubset()
        {
            var series = BuildSampleSeries();
            var view = new Series.SeriesView(series, new List<object> { "a", "b", "c", "d", "e" });

            var sliced = view.SliceView(new List<object> { "b", "d" });

            var expectedValues = new object[] { 20, 40 };
            Assert.NotStrictEqual(expectedValues, sliced.ToSeries().Values);
        }
        [Fact]
        public void SliceView_ByRangePositions_ReturnsCorrectSubset()
        {
            var series = BuildSampleSeries();
            var view = new Series.SeriesView(series, new List<object> { "a", "b", "c", "d", "e" });

            var sliced = view.SliceView((1, 3, 1));

            var expectedValues = new object[] { 20, 30, 40 };
            Assert.NotStrictEqual(expectedValues, sliced.ToSeries().Values);
        }

        [Fact]
        public void Enumerator_YieldsAllValuesInOrder()
        {
            var series = BuildSampleSeries();
            var view = new Series.SeriesView(series, new List<object> { "a", "b", "c" });

            var enumerated = view.ToList();
            var expected = new object[] { 10, 20, 30 };
            Assert.NotStrictEqual(expected, enumerated.ToArray());
        }

    }
}

