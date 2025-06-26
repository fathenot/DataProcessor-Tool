using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.Index;
namespace test.TestIndex
{
    public class TestDecimalIndex
    {
        [Fact]
        public void TestDecimalIndexCreation()
        {
            // Arrange
            var decimalValues = new decimal[] { 1.1m, 2.2m, 3.3m, 4.4m, 5.5m };
            var index = new DecimalIndex(decimalValues.ToList());
            // Act
            var count = index.Count;
            var firstValue = index.GetIndex(0);
            // Assert
            Assert.Equal(5, count);
            Assert.Equal(1.1m, firstValue);
        }

        [Fact]
        public void TestDecimalIndexContains()
        {
            // Arrange
            var decimalValues = new decimal[] { 1.1m, 2.2m, 3.3m, 4.4m, 5.5m };
            var index = new DecimalIndex(decimalValues.ToList());
            // Act
            var containsValue = index.Contains(3.3m);
            // Assert
            Assert.True(containsValue);
        }

        [Fact]
        public void TestDecimalIndexGetIndexPosition()
        {
            // Arrange
            var decimalValues = new decimal[] { 1.1m, 2.2m, 3.3m, 4.4m, 5.5m };
            var index = new DecimalIndex(decimalValues.ToList());
            // Act
            var positions = index.GetIndexPosition(3.3m);
            // Assert
            Assert.Single(positions);
            Assert.Equal(2, positions[0]);
        }
        [Fact]
        public void TestDecimalIndexSlice()
        {
            // Arrange
            var decimalValues = new decimal[] { 1.1m, 2.2m, 3.3m, 4.4m, 5.5m };
            var index = new DecimalIndex(decimalValues.ToList());
            // Act
            var slicedIndex = index.Slice(1, 4);
            // Assert
            Assert.Equal(4, slicedIndex.Count);
            Assert.Equal(2.2m, slicedIndex.GetIndex(0));
            Assert.Equal(3.3m, slicedIndex.GetIndex(1));
            Assert.Equal(4.4m, slicedIndex.GetIndex(2));

            var selectedIndex = index.Slice(new decimal[] { 1.1m, 3.3m, 4.4m }.Cast<object>().ToList());
            Assert.Equal(3, selectedIndex.Count);
            Assert.Equal(1.1m, selectedIndex.GetIndex(0));
            Assert.Equal(3.3m, selectedIndex.GetIndex(1));
            Assert.Equal(4.4m, selectedIndex.GetIndex(2));
        }
    }
}
