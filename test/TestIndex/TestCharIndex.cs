using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.Index;
namespace test.TestIndex
{
    public class TestCharIndex
    {
        // This class is intended to test the CharIndex class from DataProcessor.source.Index
        // It should contain unit tests that validate the functionality of CharIndex.
        // The tests should cover:
        // - Initialization with a list of characters
        // - Correct mapping of characters to their indices
        // - Retrieval of indices for specific characters
        // - Handling of characters not present in the index
        // - Edge cases such as empty lists or single character lists
        // Example test method (to be implemented):
        [Fact]
        public void TestInitialization()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'a' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act & Assert
            // Add assertions to verify the expected behavior
            Assert.Equal(4, charIndex.Count);
            Assert.True(charIndex.Contains('a'));
            Assert.True(charIndex.Contains('b'));
            Assert.True(charIndex.Contains('c'));
            Assert.False(charIndex.Contains('d')); // 'd' is not in the index
        }

        [Fact]
        public void TestGetIndexPosition()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'a' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            var positionsA = charIndex.GetIndexPosition('a');
            var positionsB = charIndex.GetIndexPosition('b');
            var positionsC = charIndex.GetIndexPosition('c');
            // Assert
            Assert.Equal(new List<int> { 0, 3 }, positionsA);
            Assert.Equal(new List<int> { 1 }, positionsB);
            Assert.Equal(new List<int> { 2 }, positionsC);
        }

        [Fact]
        public void TestGetIndex()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'a' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            var index0 = charIndex.GetIndex(0);
            var index1 = charIndex.GetIndex(1);
            var index2 = charIndex.GetIndex(2);
            var index3 = charIndex.GetIndex(3);
            // Assert
            Assert.Equal('a', index0);
            Assert.Equal('b', index1);
            Assert.Equal('c', index2);
            Assert.Equal('a', index3);
        }

        [Fact]
        public void TestDistinctIndices()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'a' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            var distinctChars = charIndex.DistinctIndices();
            // Assert
            Assert.Equal(new List<char> { 'a', 'b', 'c' }, distinctChars.Cast<char>().ToList());
        }

        [Fact]
        public void TestFirstPositionOf()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'a' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            var firstPositionA = charIndex.FirstPositionOf('a');
            var firstPositionB = charIndex.FirstPositionOf('b');
            var firstPositionC = charIndex.FirstPositionOf('c');
            // Assert
            Assert.Equal(0, firstPositionA);
            Assert.Equal(1, firstPositionB);
            Assert.Equal(2, firstPositionC);
        }

        [Fact]
        public void TestSlice()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'd', 'e' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            var slicedIndex = charIndex.Slice(1, 3);
            // Assert
            Assert.Equal(new List<char> { 'b', 'c', 'd' }, slicedIndex.Cast<char>().ToList());

            slicedIndex = charIndex.Slice(4, 0, -2);
            Assert.Equal(new List<char> { 'e', 'c', 'a' }, slicedIndex.Cast<char>().ToList());

            slicedIndex = charIndex.Slice(new List<object> { 'a', 'd'});
            Assert.Equal(new List<char> { 'a', 'd' }, slicedIndex.Cast<char>().ToList());
        }

        [Fact]
        public void TestSliceWithEmptyList()
        {
            // Arrange
            var chars = new List<char>();
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => charIndex.Slice(0, 0));
        }

        [Fact]
        public void TestSliceWithNull()
        {
            // Arrange
            var chars = new List<char> { 'a', 'b', 'c', 'd', 'e' };
            var charIndex = new DataProcessor.source.Index.CharIndex(chars);
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => charIndex.Slice(new List<object> { null }));
        }
    }
}
