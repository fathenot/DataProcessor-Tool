using DataProcessor.source.GenericsSeries;
using DataProcessor.source.NonGenericsSeries;
namespace test.TestNonGenericsSeries
{
    public class TestAccessAndSearch
    {
        private Series CreateSeries(int size = 10)
        {
            var values = new List<object?>();
            for (int i = 0; i < size; i++)
                values.Add(i);

            var indices = new List<object>();
            for (int i = 0; i < size; i++)
                indices.Add(i);

            return new Series(values, index: indices);
        }

        [Fact]
        public void Head_ReturnsFirstNElements()
        {
            var series = CreateSeries();
            var head = series.Head(3);

            Assert.Equal(3, head.Count);
            Assert.Equal(0, head.GetValueIntloc(0));
            Assert.Equal(1, head.GetValueIntloc(1));
            Assert.Equal(2, head.GetValueIntloc(2));
        }

        [Fact]
        public void Tail_ReturnsLastNElements()
        {
            var series = CreateSeries();
            var tail = series.Tail(3);

            Assert.Equal(3, tail.Count);
            Assert.Equal(7, tail.GetValueIntloc(0));
            Assert.Equal(8, tail.GetValueIntloc(1));
            Assert.Equal(9, tail.GetValueIntloc(2));
        }

        [Fact]
        public void GetView_ByList_ReturnsCorrectView()
        {
            var series = CreateSeries();
            var view = series.GetView(new List<object> { 1, 3, 5 });

            Assert.Equal(3, view.Count);
            var values = new List<object?>(view);
            Assert.Equal(new object?[] { 1, 3, 5 }, values.ToArray());
        }

        [Fact]
        public void Filter_ReturnsCorrectElements()
        {
            var series = CreateSeries();
            var filtered = series.Filter(v => ((int)v! % 2) == 0);

            Assert.Equal(5, filtered.Count);
            foreach (var v in filtered)
            {
                Assert.True((int)v % 2 == 0);
            }
        }

        [Fact]
        public void Find_ReturnsCorrectIndices()
        {
            var series = CreateSeries();
            var indices = series.Find(5);

            Assert.Single(indices);
            Assert.Equal(5, indices[0]);

            var notFound = series.Find(100);
            Assert.Empty(notFound);
        }

        [Fact]
        public void Contains_ReturnsCorrectResult()
        {
            var series = CreateSeries();

            Assert.True(series.Contains(3));
            Assert.False(series.Contains(100));
        }

        [Fact]
        public void GetValueIntloc_ReturnsCorrectValue()
        {
            var series = CreateSeries();

            for (int i = 0; i < series.Count; i++)
            {
                Assert.Equal(i, series.GetValueIntloc(i));
            }
        }

        [Fact]
        public void Head_Throws_OnNegativeCount()
        {
            var series = CreateSeries();
            Assert.Throws<ArgumentOutOfRangeException>(() => series.Head(-1));
        }

        [Fact]
        public void Tail_Throws_OnNegativeCount()
        {
            var series = CreateSeries();
            Assert.Throws<ArgumentOutOfRangeException>(() => series.Tail(-1));
        }

        [Fact]
        public void GetView_Throws_OnInvalidSlice()
        {
            var series = CreateSeries();
            Assert.Throws<ArgumentException>(() => series.GetView((start: 100, end: 1, step: 1)));
            Assert.Throws<ArgumentException>(() => series.GetView((start: 1, end: 3, step: 0)));
        }

        [Fact]
        public void TestFindNull1()
        {
            List<object?> data = new List<object?> { 1, null, "abc", null, 42, null };

            // Yêu cầu: tìm tất cả index có giá trị null

            var series = new Series(data);

            var nullPos = series.Find(null);

            Assert.True(nullPos.SequenceEqual(new List<int> { 1, 3, 5 }));
        }

        [Fact]
        public void FindNull2()
        {
            List<object?> data = new List<object?>
            {
                42,
                null,
                "Hello",
                DateTime.Now,
                null,
                3.14,
                null,
                Guid.NewGuid(),
                "World",
                null
            };

            // Nhiệm vụ: tìm index của tất cả phần tử null
            var series = new Series(data);
            var nullPos = series.Find(null);
            Assert.True(nullPos.SequenceEqual(new List<int> { 1,4, 6,9 }));

        }
    }
}
