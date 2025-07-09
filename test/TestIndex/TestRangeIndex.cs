namespace test.TestIndex
{
    public class TestRangeIndex
    {
        [Fact]
        public void TestRangeIndexCreation()
        {
            // Arrange
            var rangeValues = new List<int> { 1, 2, 3, 4, 5 };
            var index = new DataProcessor.source.Index.RangeIndex(1, 5);

            // Act
            var count = index.Count;
            var firstValue = index.GetIndex(0);

            // Assert
            Assert.Equal(5, count);
            Assert.Equal(1, (int)firstValue);
        }

        [Fact]
        public void TestRangeIndexContains()
        {
            // Arrange
            var index = new DataProcessor.source.Index.RangeIndex(1, 5);

            // Act
            var containsValue = index.Contains(3);

            // Assert
            Assert.True(containsValue);
        }

        [Fact]
        public void TestRangeIndexDoesNotContain()
        {
            // Arrange
            var index = new DataProcessor.source.Index.RangeIndex(1, 5);

            // Act
            var containsValue = index.Contains(6);

            // Assert
            Assert.False(containsValue);
        }

        [Fact]
        public void TestRangeIndexGetIndexPosition()
        {
            // Arrange
            var index = new DataProcessor.source.Index.RangeIndex(1, 250, 8);
            // Act
            var positions = index.GetIndexPosition(17);
            // Assert
            Assert.Single(positions);
            Assert.Equal(2, positions[0]);
        }

        [Fact]
        public void TestApplyLinq()
        {
            var index = new DataProcessor.source.Index.RangeIndex(1, 251, 8);
            // Act
            var result = index.Select(x => (int)x * 2).ToList();
            // Assert
            Assert.Equal(32, result.Count);
            Assert.Equal(2, result[0]);
            Assert.Equal(249 * 2, result[31]);
        }
    }
}
