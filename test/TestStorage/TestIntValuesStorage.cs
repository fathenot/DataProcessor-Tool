using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.ValueStorage;
namespace test.TestStorage
{
    public class TestIntValuesStorage
    {
        [Fact]
        public void TestNullHandlingInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { null, 3, null });
            Assert.Equal(3, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(intStorage.GetValue(0));
            Assert.Equal(3, (long)intStorage.GetValue(1)!);
        }

        [Fact]
        public void TestEmptyIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { });
            Assert.Equal(0, intStorage.Count);
            Assert.Empty(intStorage.NullIndices);
        }

        [Fact]
        public void TestSingleNullValueInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { null });
            Assert.Equal(1, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 0 }));
            Assert.Null(intStorage.GetValue(0));
        }

        [Fact]
        public void TestSingleValueInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { 5 });
            Assert.Equal(1, intStorage.Count);
            Assert.Empty(intStorage.NullIndices);
            Assert.Equal(5, (long)intStorage.GetValue(0));
        }

        [Fact]
        public void TestMixedValuesInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { 1, null, 3, null, 5 });
            Assert.Equal(5, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 1, 3 }));
            Assert.Equal(1, (long)intStorage.GetValue(0));
            Assert.Null(intStorage.GetValue(1));
            Assert.Equal(3, (long)intStorage.GetValue(2));
            Assert.Null(intStorage.GetValue(3));
            Assert.Equal(5, (long)intStorage.GetValue(4));
        }
        [Fact]
        public void TestNegativeValuesInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { -1, -2, null, -4 });
            Assert.Equal(4, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 2 }));
            Assert.Equal(-1, (long)intStorage.GetValue(0));
            Assert.Equal(-2, (long)intStorage.GetValue(1));
            Assert.Null(intStorage.GetValue(2));
            Assert.Equal(-4, (long)intStorage.GetValue(3));
        }

        [Fact]
        public void TestLargeValuesInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { 1000000000, 2000000000, null, 4000000000 });
            Assert.Equal(4, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 2 }));
            Assert.Equal(1000000000, (long)intStorage.GetValue(0));
            Assert.Equal(2000000000, (long)intStorage.GetValue(1));
            Assert.Null(intStorage.GetValue(2));
            Assert.Equal(4000000000, (long)intStorage.GetValue(3));
        }
        [Fact]
        public void TestSetValueInIntStorage()
        {
            var intStorage = new Int64ValuesStorage(new long?[] { null, null });
            intStorage.SetValue(0, 10);
            intStorage.SetValue(1, 20);
            Assert.Equal(10, (long)intStorage.GetValue(0));
            Assert.Equal(20, (long)intStorage.GetValue(1));

            // Test setting a null value
            intStorage.SetValue(0, null);
            Assert.Null(intStorage.GetValue(0));

            // Test setting an invalid type
            Assert.Throws<ArgumentException>(() => intStorage.SetValue(1, "invalid"));

            // Test accessing out of bounds index
            Assert.Throws<ArgumentOutOfRangeException>(() => intStorage.GetValue(2));
        }

        [Fact]
        public void TestMillionsElementsInIntStorage()
        {
            var millionElements = new long?[1000000];
            for (int i = 0; i < millionElements.Length; i++)
            {
                millionElements[i] = i % 2 == 0 ? (long?)i : null; // Half null, half int
            }
            var intStorage = new Int64ValuesStorage(millionElements);
            Assert.Equal(1000000, intStorage.Count);
            Assert.Equal(500000,intStorage.NullIndices.Count()); // Half should be null
            Assert.Equal(0, (long)intStorage.GetValue(0));
            Assert.Null(intStorage.GetValue(1));
            Assert.Equal(2, (long)intStorage.GetValue(2));
            //Apply Linq
            intStorage.NullIndices.ToList().ForEach(index =>
            {
                Assert.True(intStorage.GetValue(index) is null);
            });

        }

        // stress test
        [Fact]
        public void TestIntStorage_ComplexLinqFilter()
        {
            var millionElements = new long?[10_000_000];
            for (int i = 0; i < millionElements.Length; i++)
            {
                // Mix nulls + số âm + số dương + số chẵn/lẻ
                millionElements[i] = (i % 3 == 0) ? null : (long?)(i % 2 == 0 ? -i : i);
            }

            var intStorage = new Int64ValuesStorage(millionElements);
            Assert.Equal(10_000_000, intStorage.Count);

            // Bắt đầu đo thời gian lọc
            var sw = Stopwatch.StartNew();

            var filtered = Enumerable.Range(0, intStorage.Count)
                .Where(i =>
                {
                    var v = intStorage.GetValue(i);
                    // Logic phức tạp hơn bình thường
                    return v != null &&
                           (long)v % 7 != 0 &&         // Lẻ
                           (long)v > 10000 &&          // Lớn hơn 10k
                           (long)v < 900000 &&         // Nhỏ hơn 900k
                            v.ToString().Contains('7'); // Có số 7 trong chuỗi
                })
                .ToArray();

            sw.Stop();

            Console.WriteLine($"Filtered {filtered.Length} items in {sw.ElapsedMilliseconds} ms");

            // Đảm bảo không có phần tử null
            foreach (var idx in filtered)
            {
                Assert.NotNull(intStorage.GetValue(idx));
            }
        }
    }
}