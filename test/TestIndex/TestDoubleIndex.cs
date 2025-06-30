namespace test.TestIndex
{
    public class TestDoubleIndex
    {
        // This test checks the creation and basic functionality of a DoubleIndex.
        // It verifies that the index can be created, counts the number of elements,
        // and retrieves the first value correctly.
        [Fact]
        public void TestIndexCreation()
        {
            // Arrange
            var doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());

            // Act
            var count = index.Count;
            var firstValue = index.GetIndex(0);

            // Assert
            Assert.Equal(5, count);
            Assert.Equal(1.1, firstValue);
        }

        [Fact]
        public void TestIndexContains()
        {
            // Arrange
            var doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var containsValue = index.Contains(3.3);
            // Assert
            Assert.True(containsValue);
        }

        [Fact]
        public void TestIndexIndexOf()
        {
            var doubleValues = new double[] {
                123.456,
                789.012,
                345.678,
                901.234,
                567.890
            };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var positions = index.GetIndexPosition(345.678);
            // Assert
            Assert.Single(positions);
            Assert.Equal(2, positions[0]);
        }

        [Fact]
        public void TestIndexIndexOf2()
        {
            var doubleValues = new double[] {
                419847.179854, 981759817.1414,1412334.32541,1413145.534,1243134.125312, 1413145.534 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var positions = index.GetIndexPosition(1413145.534);
            // Assert
            Assert.Equal(2, positions.Count);
            Assert.Equal(3, positions[0]);
            Assert.Equal(5, positions[1]);
        }

        [Fact]
        public void TestIndexSlice()
        {
            // Arrange
            var doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5,7.7, 6.8 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var slicedIndex = index.Slice(1, 4, 1);
            // Assert
            Assert.Equal(4, slicedIndex.Count);
            Assert.Equal(2.2, slicedIndex.GetIndex(0));
            Assert.Equal(3.3, slicedIndex.GetIndex(1));
            var newSlicedIndex = index.Slice(new List<object> { 1.1, 3.3, 5.5 });
            Assert.Equal(3, newSlicedIndex.Count);
            Assert.Equal(1.1, newSlicedIndex.GetIndex(0));
            Assert.Equal(3.3, newSlicedIndex.GetIndex(1));
            Assert.Equal(5.5, newSlicedIndex.GetIndex(2));

        }
        [Fact]
        public void TestIndexCount()
        {
            // Arrange
            var doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var count = index.Count;
            // Assert
            Assert.Equal(5, count);
        }

        [Fact]
        public void TestApplyLinq()
        {
            // Arrange
            var doubleValues = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5, 7.7, 6.8 };
            var index = new DataProcessor.source.Index.DoubleIndex(doubleValues.ToList());
            // Act
            var filteredValues = index.Where(x => (double)x > 3.0).ToList();
            // Assert
            Assert.Equal(5, filteredValues.Count);
            Assert.Contains(4.4, filteredValues);
            Assert.Contains(5.5, filteredValues);
        }

    }
}