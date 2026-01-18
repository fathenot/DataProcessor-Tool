using DataProcessor.source.IndexTypes;
namespace test.TestIndex
{
    public class TestDateTimeIndex
    {
        [Fact]
        public void TestDateTimeIndexCreation()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            Assert.Equal(2, dateTimeIndex.Count);
            Assert.Contains(new DateTime(2023, 10, 1), dateTimeIndex);
            Assert.Contains(new DateTime(2023, 10, 2), dateTimeIndex);
        }

        [Fact]
        public void TestDateTimeIndexContains()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            // Act & Assert
            Assert.True(dateTimeIndex.Contains(new DateTime(2023, 10, 1)));
            Assert.False(dateTimeIndex.Contains(new DateTime(2023, 10, 3)));
        }

        [Fact]
        public void TestGetIndexPosition()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            // Act
            var positions = dateTimeIndex.GetIndexPosition(new DateTime(2023, 10, 1));
            // Assert
            Assert.Single(positions);
            Assert.Equal(0, positions[0]);
        }

        [Fact]
        public void TestGetIndexPositionThrowsKeyNotFoundException()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => dateTimeIndex.GetIndexPosition(new DateTime(2023, 10, 3)));
        }

        [Fact]
        public void TestFirstPositionOf()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            // Act
            var position = dateTimeIndex.FirstPositionOf(new DateTime(2023, 10, 1));
            // Assert
            Assert.Equal(0, position);
        }

        [Fact]
        public void TestSlicingRange()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2), new DateTime(2023, 10, 3) });
            // Act
            var slicedIndex = dateTimeIndex.Slice(1, 2);
            var slicedIndex2 = dateTimeIndex.TakeKeys(new List<DateTime>{ new DateTime(2023, 10, 1), new DateTime(2023, 10, 2)}.Cast<object>().ToList());
            // Assert
            Assert.Equal(2, slicedIndex.Count);
            Assert.Contains(new DateTime(2023, 10, 2), slicedIndex);
            Assert.Contains(new DateTime(2023, 10, 3), slicedIndex);
        }

        [Fact]
        public void TestSlicingRangeWithStep()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2), new DateTime(2023, 10, 3) });
            // Act
            var slicedIndex = dateTimeIndex.Slice(0, 2, 2);
            // Assert
            Assert.Equal(2, slicedIndex.Count);
            Assert.Contains(new DateTime(2023, 10, 1), slicedIndex);
            Assert.Contains(new DateTime(2023, 10, 3), slicedIndex);
        }
        [Fact]
        public void TestSlicingRangeWithInvalidStep()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2), new DateTime(2023, 10, 3) });
            // Act & Assert
            Assert.Throws<ArgumentException>(() => dateTimeIndex.Slice(0, 3, 0));
        }
        [Fact]
        public void TestGetIndex()
        {
            // Arrange
            var dateTimeIndex = new DateTimeIndex(new List<DateTime> { new DateTime(2023, 10, 1), new DateTime(2023, 10, 2) });
            // Act
            var index = dateTimeIndex.GetIndex(1);
            // Assert
            Assert.Equal(new DateTime(2023, 10, 2), index);
        }

    }
}
