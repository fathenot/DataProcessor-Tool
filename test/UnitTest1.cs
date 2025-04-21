using DataProcessor.source.NonGenericsSeries;
namespace test
{
    public class TestNonGenericsSeries
    {
        [Fact]
        public void TestAddNullValue()
        {
            Series series = new Series();
            series.Add(null);
            Assert.Single(series);
            Assert.Null(series[0][0]);
        }

        [Fact]
        public void TestAddSpecialNumbers()
        {
            Series series = new Series();
            series.Add(double.NaN);
            series.Add(double.PositiveInfinity);
            series.Add(double.NegativeInfinity);

            Assert.Equal(3, series.Count);
            Assert.True(double.IsNaN((double)series[0][0]));
            Assert.Equal(double.PositiveInfinity, series[1][0]);
            Assert.Equal(double.NegativeInfinity, series[2][0]);
            Assert.Equal(typeof(double), series.DataType);
        }
        [Fact]
        public void TestTypeInfer1()
        {
            Series series = new Series(new List<object?> { 1, 2, 3, 5, 6, 7, 7 });
            Assert.Equal(typeof(long), series.DataType);
        }

        [Fact]
        public void TestTypeInfer2()
        {
            Series series = new Series(new List<object?> { 1, 2, 3, 5, 6, 7, (Int128)123 });
            Assert.Equal(typeof(Int128), series.DataType);
        }

        [Fact]
        public void TestRemoveNullValue()
        {
            Series series = new Series();
            Assert.False(series.RemoveAllOccurent(null));
        }
        [Fact]
        public void RemoveAllOccurence()
        {
            Series series = new Series(values: new List<object?> { 1, 2, 34, 5, 7, 7 });
            series.RemoveAllOccurent(7);
            Assert.False(series.Contains(7));
        }
        [Fact]
        public void TestIndexOutOfRangeThrows()
        {
            Series series = new Series();
            series.Add("hello");

            Assert.Throws<ArgumentOutOfRangeException>(() => series[1]);
        }

        [Fact]
        public void TestSetValueAtIndex()
        {
            Series series = new Series();
            series.Add("abc");
            series[0][0] = "xyz";

            Assert.Equal("abc", series[0][0]);
        }

        [Fact]
        public void TestCopyConstructorPreservesValues()
        {
            Series original = new Series();
            original.Add(1);
            original.Add(2);

            Series copy = new Series(original);
            Assert.Equal(2, copy.Count);
            Assert.Equal(1, copy[0][0]);
            Assert.Equal(2, copy[1][0]);
        }

        [Fact]
        public void TestRemoveValue()
        {
            Series series = new Series();
            series.Add(1);
            series.Add(2);
            series.Add(3);

            series.RemoveAllOccurent(2);
            Assert.Equal(2, series.Count);
            Assert.DoesNotContain(2, series.Values);
        }

        [Fact]
        public void TestClearSeries()
        {
            Series series = new Series();
            series.Add("a");
            series.Add("b");

            series.Clear();
            Assert.Empty(series);
        }

        [Fact]
        public void TestChangeIndex()
        {
            Series series = new Series(new List<object?> { 10, 20, 30 }, seriesName: "test", index: new List<object> { "a", "b", "c" });

            Assert.Equal("a", series.Index[0]);
            Assert.Equal("b", series.Index[1]);
            Assert.Equal(20, series["b"][0]);
        }

        [Fact]
        public void TestSeriesIndex()
        {
            var values = new List<object?> { "A", "B", "C" };
            var index = new List<object> { 101, 102, 103 };
            var series = new Series(values: values, seriesName: "letters", index: index);

            Assert.Equal("B", series[102][0]);
            Assert.Equal(3, series.Count);
            Assert.Equal("letters", series.Name);
        }

        [Fact]
        public void TestContainsAndIndexOf()
        {
            var series = new Series(values: new List<object?> { "apple", "banana", "cherry" }, index: new List<object> { 1, 2, 3 });

            Assert.True(series.Contains("banana"));
            Assert.Contains(2, series.Find("cherry"));
        }

        [Fact]
        public void TestViewReflectsSourceChange()
        {
            var series = new Series(values: new List<object?> { 1, 2, 3 }, index: new List<object> { "a", "b", "c" });
            var view = series.GetView(new List<object> { "b", "c" });

            series.Add(4, "d");

            Assert.Equal(2, view.Count); // Không thay đổi vì "d" không nằm trong view index
            Assert.Equal(4, series.Count);
        }

        [Fact]
        public void TestViewReflectsViewChange()
        {
            var series = new Series(values: new List<object?> { 1, 2, 3 }, index: new List<object> { "a", "b", "c" });
            var view = series.GetView(new List<object> { "b", "c" });

            series.Add(4, "b");

            Assert.Equal(2, view.Count); // Không thay đổi vì "b" được thêm sau khi tạo view và cần sync một cách thủ công
            view = series.GetView(new List<object> { "b", "c" });
            Assert.Equal(3, view.Count);
        }

        [Fact]
        public void TestViewMethods1()
        {
            var series = new Series(values: new List<object?> { 1, 2, 3, 4, 365, 646, 2523, 346235, 263 }, index: new List<object> { "a", "b", "c", "a", "a", "b", "b" });
            var view = series.GetView(("a", "b", 1));
            List<object?> expcetedElements = new List<object?> { 1, 2, 4, 365, 646, 2523 };
            Assert.Equal(expcetedElements, view.ToSeries().Values);
            Assert.Equal(6, view.Count);
            Assert.Equal(new List<object> { "a", "b", "a", "a", "b", "b" }, view.Index);
        }

    }
}
