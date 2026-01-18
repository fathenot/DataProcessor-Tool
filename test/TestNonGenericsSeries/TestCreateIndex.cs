using DataProcessor.source.IndexTypes;
using DataProcessor.source.NonGenericsSeries;
using Xunit;
using System;
using System.Linq;

namespace test.TestNonGenericsSeries
{
    public class TestCreateIndex
    {
        [Fact]
        public void Create_Int64Index()
        {
            var values = new object[] { 1L, 2L, 3L }.ToList();
            var index = Series.CreateIndex(values);

            Assert.IsType<Int64Index>(index);
            Assert.Equal(3, index.Count);
        }

        [Fact]
        public void Create_StringIndex()
        {
            var values = new object[] { "a", "b", "c" }.ToList();
            var index = Series.CreateIndex(values);

            Assert.IsType<StringIndex>(index);
            Assert.Equal(3, index.Count);
        }

        [Fact]
        public void Create_DoubleIndex()
        {
            var values = new object[] { 1.1, 2.2, 3.3 }.ToList();
            var index = Series.CreateIndex(values);

            Assert.IsType<DoubleIndex>(index);
            Assert.Equal(3, index.Count);
        }

        [Fact]
        public void Create_DateTimeIndex()
        {
            var values = new object[]
            {
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 2)
            }.ToList();

            var index = Series.CreateIndex(values);

            Assert.IsType<DateTimeIndex>(index);
            Assert.Equal(2, index.Count);
        }

        [Fact]
        public void Create_CharIndex()
        {
            var values = new object[] { 'a', 'b' }.ToList();
            var index = Series.CreateIndex(values);

            Assert.IsType<CharIndex>(index);
            Assert.Equal(2, index.Count);
        }

        [Fact]
        public void Create_DecimalIndex()
        {
            var values = new object[] { 1.1m, 2.2m, 3.3m }.ToList();
            var index = Series.CreateIndex(values);

            Assert.IsType<DecimalIndex>(index);
            Assert.Equal(3, index.Count);
        }

        [Fact]
        public void Create_ObjectIndex_When_MixedTypes()
        {
            var values = new object[]
            {
                "a",
                1,
                2.2,
                new DateTime(2020, 1, 1)
            }.ToList();

            var index = Series.CreateIndex(values);

            Assert.IsType<ObjectIndex>(index);
            Assert.Equal(4, index.Count);
        }
    }
}
