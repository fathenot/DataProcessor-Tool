using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.EngineWrapper.SortingEngine;
namespace test.TestEngine
{
    public class TestSortEngine
    {
        [Theory]
        [InlineData(true, new[] { 2, 1, 3 }, new object[] { "b", "a", "c" }, new[] { 1, 2, 3 })]
        [InlineData(false, new[] { 2, 1, 3 }, new object[] { "b", "a", "c" }, new[] { 3, 2, 1 })]
        public void SortByIndex_SortsCorrectly(bool ascending, int[] input, object[] index, int[] expected)
        {
            // Arrange
            var array = (int[])input.Clone();
            var indexList = new List<object>(index);

            // Act
            IndexValueSorter.SortByIndex(array, indexList, ascending);

            // Assert
            Assert.Equal(expected, array);
        }

        [Theory]
        [InlineData(true, new[] { 5, 3, 8, 1 }, new[] { "a", "b", "c", "d" }, new[] { 1, 3, 5, 8 })]
        [InlineData(false, new[] { 5, 3, 8, 1 }, new[] { "a", "b", "c", "d" }, new[] { 8, 5, 3, 1 })]
        public void SortByValue_SortsCorrectly(bool ascending, int[] input, object[] index, int[] expected)
        {
            // Arrange
            var array = (int[])input.Clone();
            var indexList = new List<object>(index);

            // Act
            IndexValueSorter.SortByValue(array, indexList, ascending);

            // Assert
            Assert.Equal(expected, array);
        }

        [Fact]
        public void SortByIndex_EmptyArray_NoException()
        {
            var array = Array.Empty<int>();
            var index = new List<object>();

            IndexValueSorter.SortByIndex(array, index);

            Assert.Empty(array);
        }

        [Fact]
        public void SortByValue_OneElementArray_NoChange()
        {
            var array = new[] { 42 };
            var index = new List<object> { "x" };

            IndexValueSorter.SortByValue(array, index);

            Assert.Single(array);
            Assert.Equal(42, array[0]);
        }

        [Fact]
        public void SortByIndex_WithNumericIndex_SortsCorrectly()
        {
            var array = new[] { "c", "a", "b" };
            var index = new List<object> { 3, 1, 2 };

            IndexValueSorter.SortByIndex(array, index, ascending: true);

            Assert.Equal(new[] { "a", "b", "c" }, array);
        }

        [Fact]
        public void SortByIndex_WithMixedObjectIndex_SortsUsingComparer()
        {
            var array = new[] { "c", "a", "b" };
            var index = new List<object> { "3", "1", "2" }; // all string numbers

            IndexValueSorter.SortByIndex(array, index);

            Assert.Equal(new[] { "a", "b", "c" }, array);
        }

        [Fact]
        public void SortByValue_WithCustomObjects_SortsByToStringOrComparable()
        {
            var obj1 = new Dummy(5);
            var obj2 = new Dummy(2);
            var obj3 = new Dummy(9);

            var array = new[] { obj1, obj2, obj3 };
            var index = new List<object> { 0, 1, 2 };

            IndexValueSorter.SortByValue(array, index, ascending: true);

            Assert.Equal(new[] { obj2, obj1, obj3 }, array);
        }

        private class Dummy : IComparable
        {
            public int Value { get; }

            public Dummy(int value) => Value = value;

            public int CompareTo(object obj)
            {
                if (obj is Dummy d)
                    return Value.CompareTo(d.Value);
                return -1;
            }

            public override string ToString() => Value.ToString();
        }
    }
}
