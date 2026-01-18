using DataProcessor.source.IndexTypes;
namespace test.TestIndex
{
    public class TestObjectIndex
    {
        [Fact]
        public void TestObjectIndexCretations()
        {
            var arg = new List<object> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, "satring", new Guid() };
            var index = new ObjectIndex(arg);
            Assert.Equal(arg.Count, index.Count);
            for (int i = 0; i < arg.Count; i++)
            {
                Assert.Equal(arg[i].ToString(), index.GetIndex(i).ToString());
            }

        }

        [Fact]
        public void TestObjectIndexEmpty()
        {
            List<object> arg = new List<object>();
            var index = new ObjectIndex(arg);
            Assert.False(index.Contains(0));
            Assert.Empty(index);
        }

        [Fact]
        public void TestIndexSlice()
        {
            // Arrange
            var doubleValues = new Object[] { 1.1, 2.2, 3.3, 4.4, 5.5, 7.7, 6.8 };
            var index = new ObjectIndex(doubleValues.Cast<object>().ToList());
            // Act
            var slicedIndex = index.Slice(1, 4, 1);
            // Assert
            Assert.Equal(4, slicedIndex.Count);
            Assert.Equal(2.2, slicedIndex.GetIndex(0));
            Assert.Equal(3.3, slicedIndex.GetIndex(1));
            var newSlicedIndex = index.TakeKeys(new List<object> { 1.1, 3.3, 5.5 });
            Assert.Equal(3, newSlicedIndex.Count);
            Assert.Equal(1.1, newSlicedIndex.GetIndex(0));
            Assert.Equal(3.3, newSlicedIndex.GetIndex(1));
            Assert.Equal(5.5, newSlicedIndex.GetIndex(2));

        }
        [Fact]
        public void TestIndexSlice_StepGreaterThanOne()
        {
            var values = new object[] { "a", "b", "c", "d", "e", "f" };
            var index = new ObjectIndex(values.ToList());

            var sliced = index.Slice(0, 5, 2); // Chọn a, c, e

            Assert.Equal(3, sliced.Count);
            Assert.Equal("a", sliced.GetIndex(0).ToString());
            Assert.Equal("c", sliced.GetIndex(1).ToString());
            Assert.Equal("e", sliced.GetIndex(2).ToString());
        }

        [Fact]
        public void TestIndexSlice_StepNegative()
        {
            var values = new object[] { 10, 20, 30, 40, 50 };
            var index = new ObjectIndex(values.ToList());

            var sliced = index.Slice(4, 0, -2); // 50, 30

            Assert.Equal(3, sliced.Count);
            Assert.Equal(50.ToString(), sliced.GetIndex(0).ToString());
            Assert.Equal(30.ToString(), sliced.GetIndex(1).ToString());
        }
        [Fact]
        public void TestIndexSlice_EmptyResult()
        {
            var values = new object[] { 1, 2, 3, 4, 5 };
            var index = new ObjectIndex(values.ToList());

            var sliced = index.Slice(3, 3, 1); // start == end

            Assert.Single(sliced);
            Assert.Equal(1, sliced.Count);
        }
        [Fact]
        public void TestIndexSlice_ByListWithNonExistentValue_ShouldThrow()
        {
            var values = new object[] { "x", "y", "z" };
            var index = new ObjectIndex(values.ToList());

            var sliceKeys = new List<object> { "x", "a", "z" }; // "a" không có

            Assert.Throws<ArgumentException>(() => index.TakeKeys(sliceKeys));
        }

        [Fact]
        public void TestIndexSlice_ByListWithNullValue_ShouldThrow()
        {
            var values = new object[] { "x", "y", "z" };
            var index = new ObjectIndex(values.ToList());
            var sliceKeys = new List<object> { "x", null, "z" }; // null không hợp lệ
            Assert.Throws<ArgumentException>(() => index.TakeKeys(sliceKeys));
        }

        [Fact]
        public void TestIndexPositions()
        {
            // Arrange
            var values = new object[] { "apple", "banana", "cherry", "date", "elderberry" };
            var index = new ObjectIndex(values.ToList());
            // Act
            var positions = index.GetIndexPosition("cherry");
            // Assert
            Assert.Single(positions);
            Assert.Equal(2, positions[0]); // cherry is at index 2
            Assert.Equal(5, index.Count); // Ensure the count is correct
            Assert.Equal(2, index.FirstPositionOf("cherry")); // Ensure FirstPositionOf works correctly
        }

        [Fact]
        public void TestDisstinctIndices()
        {
            // Arrange
            var values = new object[] { "apple", "banana", "cherry", "apple", "banana" };
            var index = new ObjectIndex(values.ToList());
            // Act
            var distinctIndices = index.DistinctIndices().ToList();
            // Assert
            Assert.Equal(3, distinctIndices.Count); // apple, banana, cherry
            Assert.Contains("apple", distinctIndices);
            Assert.Contains("banana", distinctIndices);
            Assert.Contains("cherry", distinctIndices);
        }

        [Fact]
        public void TestApplyLinq()
        {
            // Arrange
            var values = new object[] { 1, 2, 3, 4, 5 };
            var index = new ObjectIndex(values.ToList());
            // Act
            var result = index.Select(x => (int)x * 2).ToList();
            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal(2, result[0]);
            Assert.Equal(4, result[1]);
            Assert.Equal(6, result[2]);
            Assert.Equal(8, result[3]);
            Assert.Equal(10, result[4]);
        }
    }
}
