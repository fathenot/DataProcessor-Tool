using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.ValueStorage;
namespace test.TestStorage
{
    public class TestCharStorage
    {
        [Fact]
        public void TestCharStorageWithNulls()
        {
            var charStorage = new CharStorage(new char?[] { null, 'a', null, 'b' });
            Assert.Equal(4, charStorage.Count);
            Assert.True(charStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(charStorage.GetValue(0));
            Assert.Equal('a', charStorage.GetValue(1));
            Assert.Null(charStorage.GetValue(2));
            Assert.Equal('b', charStorage.GetValue(3));
        }

        [Fact]
        public void TestCharStorageWithAllNulls()
        {
            var charStorage = new CharStorage(new char?[] { null, null, null });
            Assert.Equal(3, charStorage.Count);
            Assert.True(charStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(charStorage.GetValue(0));
            Assert.Null(charStorage.GetValue(1));
            Assert.Null(charStorage.GetValue(2));
        }

        [Fact]
        public void TestCharStorageWithEmptyArray()
        {
            var charStorage = new CharStorage(new char?[] { });
            Assert.Equal(0, charStorage.Count);
            Assert.Empty(charStorage.NullIndices);
        }

        [Fact]
        public void TestCharStorageSetValue()
        {
            var charStorage = new CharStorage(new char?[] { null, null });
            charStorage.SetValue(0, 'c');
            charStorage.SetValue(1, 'd');
            Assert.Equal('c', charStorage.GetValue(0));
            Assert.Equal('d', charStorage.GetValue(1));
            Assert.Throws<ArgumentException>(() => charStorage.SetValue(0, "Not a Char"));
            Assert.Throws<IndexOutOfRangeException>(() => charStorage.GetValue(2)); // Accessing out of bounds index
        }

        [Fact]
        public void TestApplyLinq()
        {
            var charStorage = new CharStorage(new char?[] { 'x', null, 'y' });
            var result = charStorage.AsTyped<char?>().Where(c => c.HasValue).Select(c => c.Value.ToString()).ToList();
            Assert.Equal(2, result.Count);
            Assert.Contains("x", result);
            Assert.Contains("y", result);
        }

        [Fact]
        public void TestEnumerator()
        {
            var charStorage = new CharStorage(new char?[] { 'a', 'b', null, 'c' });
            var enumerator = charStorage.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                count++;
                if (count == 3)
                {
                    Assert.Null(enumerator.Current); // Should be null for the third element
                }
                else if (count == 1)
                {
                    Assert.Equal('a', enumerator.Current); // First element should be 'a'
                }
                else if (count == 2)
                {
                    Assert.Equal('b', enumerator.Current); // Second element should be 'b'
                }
                else if (count == 4)
                {
                    Assert.Equal('c', enumerator.Current); // Fourth element should be 'c'
                }
            }
            Assert.Equal(4, count); // Should skip nulls
        }

        [Fact]
        public void TestAsTyped()
        {
            var charStorage = new CharStorage(new char?[] { 'a', 'b', null, 'c' });
            var typedList = charStorage.AsTyped<char?>().ToList();
            Assert.Equal(4, typedList.Count);
            Assert.Contains('a', typedList);
            Assert.Contains('b', typedList);
            Assert.Contains('c', typedList);
            Assert.Contains(null, typedList);
        }
    }
}
