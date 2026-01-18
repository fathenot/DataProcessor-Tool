using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.IndexTypes;

namespace test.TestIndex
{
    public class TestStringIndex
    {
        [Fact]
        public void TestStringIndexOf()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal([0], index.GetIndexPosition("apple"));
            Assert.Equal([1], index.GetIndexPosition("banana"));
            Assert.Equal([2], index.GetIndexPosition("cherry"));
            Assert.Equal([3], index.GetIndexPosition("date"));
            Assert.Throws<KeyNotFoundException>(() => index.GetIndexPosition("fig"));
        }

        [Fact]
        public void TestStringIndexSlice()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            var slicedIndex = index.Slice(1, 3);
            Assert.Equal(["banana", "cherry", "date"], slicedIndex.IndexList);
        }

        [Fact]
        public void TestStringIndexContains()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.True(index.Contains("banana"));
            Assert.False(index.Contains("fig"));
        }
        [Fact]
        public void TestGetIndex()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal("apple", index.GetIndex(0));
            Assert.Equal("banana", index.GetIndex(1));
            Assert.Equal("cherry", index.GetIndex(2));
            Assert.Equal("date", index.GetIndex(3));
            Assert.Throws<IndexOutOfRangeException>(() => index.GetIndex(4));
        }

        [Fact]
        public void TestStringIndexCount()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal(4, index.Count);
        }

        [Fact]
        public void TestStringIndexNormalization()
        {
            string[] strings = { "café", "café" }; // 'é' is represented differently
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal(index.GetIndexPosition("café"), index.GetIndexPosition("café"));
            Assert.Equal(2, index.Count); // Both should normalize to the same string
        }

        [Fact]
        public void TestStringIndexNullValues()
        {
            Assert.Throws<ArgumentNullException>(() => new StringIndex(null));
            Assert.Throws<ArgumentException>(() => new DataProcessor.source.IndexTypes.StringIndex(new List<string> { "apple", null }));
        }

        [Fact]
        public void TestStringIndexEmptyList()
        {
            var index = new DataProcessor.source.IndexTypes.StringIndex(new List<string>());
            Assert.Empty(index.IndexList);
            Assert.Equal(0, index.Count);
        }

        [Fact]
        public void TestStringIndexStepZero()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Throws<ArgumentException>(() => index.Slice(0, 3, 0)); // step cannot be zero
        }

        [Fact]
        public void TestStringIndexStepNegative()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal(4, index.Slice(3, 0, -1).Count); // should return all elements in reverse order
        }

        [Fact]
        public void TestStringIndexStepPositive()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new DataProcessor.source.IndexTypes.StringIndex(strings.ToList());
            Assert.Equal(2, index.Slice(0, 3, 2).Count); // should return every second element
            Assert.Equal(["apple", "cherry"], index.Slice(0, 3, 2).IndexList);
        }

        [Fact]
        public void TestEnumerator()
        {
            string[] strings = { "apple", "banana", "cherry", "date" };
            var index = new StringIndex(strings.ToList());
            int count = 0;
            foreach (var item in index)
            {
                Assert.Contains(item, strings);
                count++;
            }
            Assert.Equal(strings.Length, count); // Ensure all items were enumerated
        }

        [Fact]
        public void TestFirstOccurrence()
        {
            string[] strings = { "apple", "banana", "cherry", "date", "banana" };
            var index = new StringIndex(strings.ToList());
            Assert.Equal(1, index.FirstPositionOf("banana")); // Should return the first occurrence
        }

        [Fact]
        public void TestDistinctValues()
        {
            string[] strings = { "apple", "banana", "cherry", "date", "banana" };
            var index = new StringIndex(strings.ToList());
            var distinctValues = index.DistinctIndices().ToList();
            Assert.Equal(4, distinctValues.Count); // Should return distinct values only
            Assert.Contains("apple", distinctValues);
            Assert.Contains("banana", distinctValues);
            Assert.Contains("cherry", distinctValues);
            Assert.Contains("date", distinctValues);
        }

    }
}