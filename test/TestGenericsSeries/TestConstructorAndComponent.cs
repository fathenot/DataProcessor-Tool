using DataProcessor.source.GenericsSeries;
using DataProcessor.source.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.TestGenericsSeries
{
    public class TestConstructorAndComponent
    {
        [Fact]
        public void Constructor_WithDataOnly_ShouldCreateWithRangeIndex()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3 };

            // Act
            var series = new Series<int>(data);

            // Assert
            Assert.Equal(3, series.Count);
            for (int i = 0; i < data.Count; i++)
            {
                Assert.Equal(data[i], series[i][0]);
            };
        }

        [Fact]
        public void Constructor_WithDataAndName_ShouldAssignName()
        {
            // Arrange
            var data = new List<string> { "a", "b" };
            var name = "MySeries";

            // Act
            var series = new Series<string>(data, name);

            // Assert
            Assert.Equal(name, series.Name);
            Assert.Equal("a", series[0][0]);
            Assert.Equal("b", series[1][0]);
        }

        [Fact]
        public void Constructor_WithDataAndIndex_ShouldRespectCustomIndex()
        {
            // Arrange
            var data = new List<double> { 3.14, 2.71 };
            var index = new List<object> { "pi", "e" };

            // Act
            var series = new Series<double>(data, index: index);

            // Assert
            Assert.Equal(2, series.Count);
            Assert.Equal(3.14, series["pi"][0]);
            Assert.Equal(2.71, series["e"][0]);
        }

        [Fact]
        public void Constructor_WithMismatchedDataAndIndexLength_ShouldThrow()
        {
            // Arrange
            var data = new List<int> { 1, 2 };
            var index = new List<object> { "onlyOne" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Series<int>(data, index: index));
        }

        [Fact]
        public void CopyConstructor_ShouldCreateIndependentCopy()
        {
            // Arrange
            var original = new Series<int>(data: new List<int> { 10, 20, 30 }, name: "numbers", null);

            // Act
            var copy = new Series<int>(original);

            // Assert
            Assert.Equal(original.Count, copy.Count);
            Assert.Equal(original.Name, copy.Name);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(copy[i][0], original.Values[i]);
            }
        }

        [Fact]
        public void Enumerator_ShouldIterateOverValues()
        {
            // Arrange
            var data = new List<int> { 42, 43, 44 };
            var series = new Series<int>(data);

            var enumerator = new Series<int>.GenericsSeriesEnumerator(series);

            // Act & Assert
            var collected = new List<int>();
            while (enumerator.MoveNext())
            {
                collected.Add(enumerator.Current);
            }

            Assert.Equal(data, collected);
        }

        [Fact]
        public void Enumerator_Reset_ShouldRestartIteration()
        {
            // Arrange
            var data = new List<string> { "x", "y" };
            var series = new Series<string>(data);
            var enumerator = new Series<string>.GenericsSeriesEnumerator(series);

            // Act
            Assert.True(enumerator.MoveNext());
            Assert.Equal("x", enumerator.Current);

            enumerator.Reset();
            Assert.True(enumerator.MoveNext());
            Assert.Equal("x", enumerator.Current);
        }
    }
}
