using DataProcessor.source.IndexTypes;
namespace test.TestIndex
{
    public class TestInt64Index
    {
        [Fact]
        public void TestInt64IndexCreation()
        {
            var indexList = new List<long> { 1, 2, 3, 4, 5 };
            var int64Index = new Int64Index(indexList);
            Assert.Equal(5, int64Index.Count);
            Assert.Equal(1, (long)int64Index.GetIndex(0));
            Assert.Equal(2, (long)int64Index.GetIndex(1));
            Assert.Equal(3, (long)int64Index.GetIndex(2));
            Assert.Equal(4, (long)int64Index.GetIndex(3));
            Assert.Equal(5, (long)int64Index.GetIndex(4));
        }

        [Fact]
        public void TestInt64IndexFirstPositionOf()
        {
            var indexList = new List<long> { 1, 2, 3, 2, 5 };
            var int64Index = new Int64Index(indexList);
            Assert.Equal(1, int64Index.FirstPositionOf(2)); // First occurrence of 2 is at index 1
            Assert.Equal(-1, int64Index.FirstPositionOf(6)); // 6 does not exist in the index
        }
        [Fact]
        public void TestInt64IndexContains()
        {
            var indexList = new List<long> { 1, 2, 3, 4, 5 };
            var int64Index = new Int64Index(indexList);
            Assert.True(int64Index.Contains(3)); // 3 exists in the index
            Assert.False(int64Index.Contains(6)); // 6 does not exist in the index
        }

        [Fact]
        public void TestInt64IndexGetPositionsOf()
        {
            var indexList = new List<long> { 1, 2, 3, 2, 5 };
            var int64Index = new Int64Index(indexList);
            var positions = int64Index.GetIndexPosition(2);
            Assert.Equal(new List<int> { 1, 3 }, positions); // 2 occurs at indices 1 and 3
            Assert.Throws<ArgumentException>(() => int64Index.GetIndexPosition(6)); // Should throw for null input
        }

        [Fact]
       public void TestInt64IndexGetEnumerator()
        {
            var indexList = new List<long> { 1, 2, 3, 4, 5 };
            var int64Index = new Int64Index(indexList);
            int count = 0;  
            foreach (var item in int64Index)
            {
                count++;
                Assert.Contains((long)item, indexList); // Each item should be in the original index list
            }
            Assert.Equal(5, count); // Ensure we iterated through all items
            Assert.Equal(5, int64Index.Count); // Ensure the count is correct
            //Assert.True(int64Index.DistinctIndices().ToList().SequenceEqual(new List<object> { 1, 2, 3, 4, 5 })); // Ensure distinct indices are correct
        }

        [Fact]
        public void TestInt64IndexConvertToLong()
        {
            var indexList = new List<long> { 1, 2, 3, 4, 5 };
            var int64Index = new Int64Index(indexList);
            Assert.Throws<ArgumentNullException>(() => int64Index.FirstPositionOf(null)); // Should throw for null input
            Assert.Throws<ArgumentException>(() => int64Index.FirstPositionOf("invalid")); // Should throw for invalid type
        }

        [Fact]
        public void TestInt64IndexWithEmptyList()
        {
            var int64Index = new Int64Index(new List<long>());
            Assert.Equal(0, int64Index.Count);
            Assert.Empty(int64Index.IndexList);
            Assert.Throws<ArgumentOutOfRangeException>(() => int64Index.GetIndex(0)); // Accessing out of bounds index
        }
    }
}
