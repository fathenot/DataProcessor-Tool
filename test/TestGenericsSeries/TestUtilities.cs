using Xunit;
using DataProcessor.source.GenericsSeries;
using DataProcessor.source.NonGenericsSeries;
using System;
using System.Collections.Generic;

namespace test.TestGenericsSeries
{
    public class TestUtilities
    {
        // ========== SORT TESTS ==========

        [Fact]
        public void Sort_WithDefaultComparer_SortsCorrectly()
        {
            var series = new Series<int>(new List<int> { 5, 3, 4 }, "nums", new List<object> { "a", "b", "c" });
            var sorted = series.Sort();
            Assert.Equal(new List<int> { 3, 4, 5 }, sorted.Values);
            Assert.Equal(new List<object> { "b", "c", "a" }, sorted.IndexList);
        }

        [Fact]
        public void Sort_WithCustomComparer_SortsByDescending()
        {
            var series = new Series<int>(new List<int> { 5, 3, 4 }, "nums", new List<object> { "a", "b", "c" });
            var sorted = series.Sort(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            Assert.Equal(new List<int> { 5, 4, 3 }, sorted.Values);
        }

        [Fact]
        public void Sort_EmptySeries_ReturnsEmpty()
        {
            var series = new Series<int>(new List<int>(), "empty", new List<object>());
            var sorted = series.Sort();
            Assert.Empty(sorted.Values);
            Assert.Empty(sorted.IndexList);
        }

        [Fact]
        public void Sort_UncomparableType_ThrowsInvalidOperation()
        {
            var series = new Series<object>(new List<object> { new object(), new object() }, "objs", new List<object> { 1, 2 });
            Assert.Throws<InvalidOperationException>(() => series.Sort());
        }

        // ========== VIEW TESTS ==========

        [Fact]
        public void GetView_ByIndexList_ValidIndices()
        {
            var series = new Series<string>(new List<string> { "x", "y", "z" }, "letters", new List<object> { "i1", "i2", "i3" });
            var view = series.GetView(new List<object> { "i3", "i1" });
            Assert.Equal(new List<string> { "z", "x" }, view.Values);
        }

        [Fact]
        public void GetView_ByIndexList_InvalidIndex_Throws()
        {
            var series = new Series<string>(new List<string> { "a", "b" }, "letters", new List<object> { "i1", "i2" });
            Assert.Throws<ArgumentException>(() => series.GetView(new List<object> { "i3" }));
        }

        [Fact]
        public void GetView_BySlice_ReturnsCorrectRange()
        {
            var series = new Series<int>(new List<int> { 10, 20, 30, 40, 50 }, "nums", new List<object> { 0, 1, 2, 3, 4 });
            var view = series.GetView((1, 3, 1));
            Assert.Equal(new List<int> { 20, 30,40 }, view.Values);
        }

        [Fact]
        public void GetView_InvalidSlice_Throws()
        {
            var series = new Series<int>(new List<int> { 10, 20, 30 }, "nums", new List<object> { 0, 1, 2 });
            Assert.Throws<ArgumentOutOfRangeException>(() => series.GetView((5, 10, 1)));
        }

        // ========== GROUP TESTS ==========

        [Fact]
        public void GroupsByIndex_CreatesCorrectGroups()
        {
            var series = new Series<int>(new List<int> { 1, 2, 3, 4 }, "nums", new List<object> { "a", "a", "b", "b" });
            var groups = series.GroupsByIndex();
            Assert.Equal(new[] { 0, 1 }, groups.Groups["a"]);
            Assert.Equal(new[] { 2, 3 }, groups.Groups["b"]);
        }

        [Fact]
        public void GroupByValue_CreatesCorrectGroups()
        {
            var series = new Series<int>(new List<int> { 1, 2, 1, 3 }, "nums", new List<object> { "i1", "i2", "i3", "i4" });
            var groups = series.GroupByValue();
            Assert.Equal(new[] { 0, 2 }, groups.Groups[1]);
            Assert.Equal(new[] { 1 }, groups.Groups[2]);
            Assert.Equal(new[] { 3 }, groups.Groups[3]);
        }

        [Fact]
        public void GroupByValue_WithDuplicateValues_GroupsCorrectly()
        {
            var series = new Series<string>(new List<string> { "a", "b", "a", "b", "c" }, "letters", new List<object> { 1, 2, 3, 4, 5 });
            var groups = series.GroupByValue();
            Assert.Equal(new[] { 0, 2 }, groups["a"]);
            Assert.Equal(new[] { 1, 3 }, groups.Groups["b"]);
            Assert.Equal(new[] { 4 }, groups.Groups["c"]);
        }

        // ========== CLONE & CONVERT TESTS ==========

        [Fact]
        public void Clone_CreatesIndependentSeries()
        {
            var series = new Series<int>(new List<int> { 1, 2, 3 }, "nums", new List<object> { "a", "b", "c" });
            var clone = series.Clone();
            Assert.Equal(series.Values, clone.Values);
            Assert.NotSame(series.Values, clone.Values);
        }

        [Fact]
        public void ConvertToNonGenerics_PreservesData()
        {
            var series = new Series<double>(new List<double> { 1.1, 2.2 }, "nums", new List<object> { "i1", "i2" });
            var nonGen = series.ConvertToNonGenerics();
            Assert.Equal(new List<object?> { 1.1, 2.2 }, nonGen.Values);
            Assert.Equal(typeof(double), nonGen.DataType);
        }
    }
}
