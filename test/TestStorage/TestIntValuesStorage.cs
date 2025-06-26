using System;
using System.Collections.Generic;
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
            var intStorage = new IntValuesStorage(new long?[] { null, 3, null });
            Assert.Equal(3, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(intStorage.GetValue(0));
            Assert.Equal(3, (long)intStorage.GetValue(1));
        }

        [Fact]
        public void TestEmptyIntStorage()
        {
            var intStorage = new IntValuesStorage(new long?[] { });
            Assert.Equal(0, intStorage.Count);
            Assert.Empty(intStorage.NullIndices);
        }

        [Fact]
        public void TestSingleNullValueInIntStorage()
        {
            var intStorage = new IntValuesStorage(new long?[] { null });
            Assert.Equal(1, intStorage.Count);
            Assert.True(intStorage.NullIndices.SequenceEqual(new[] { 0 }));
            Assert.Null(intStorage.GetValue(0));
        }

        [Fact]
        public void TestSingleValueInIntStorage()
        {
            var intStorage = new IntValuesStorage(new long?[] { 5 });
            Assert.Equal(1, intStorage.Count);
            Assert.Empty(intStorage.NullIndices);
            Assert.Equal(5, (long)intStorage.GetValue(0));
        }

        [Fact]
        public void TestMixedValuesInIntStorage()
        {
            var intStorage = new IntValuesStorage(new long?[] { 1, null, 3, null, 5 });
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
            var intStorage = new IntValuesStorage(new long?[] { -1, -2, null, -4 });
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
            var intStorage = new IntValuesStorage(new long ?[] { 1000000000, 2000000000, null, 4000000000 });
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
            var intStorage = new IntValuesStorage(new long?[] { null, null });
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
       public void RunAllTests()
        {
            TestNullHandlingInIntStorage();
            TestEmptyIntStorage();
            TestSingleNullValueInIntStorage();
            TestSingleValueInIntStorage();
            TestMixedValuesInIntStorage();
            TestNegativeValuesInIntStorage();
            TestLargeValuesInIntStorage();
            TestSetValueInIntStorage();
        }
        public class IntValuesStorageConcurrencyTests
        {
            private const int ElementCount = 1_000_000;
            private const int ThreadCount = 16;

            private IntValuesStorage CreateStorage()
            {
                var values = new long?[ElementCount];
                for (int i = 0; i < ElementCount; i++)
                {
                    values[i] = 0;
                }
                return new IntValuesStorage(values);
            }

            [Fact]
            public void Parallel_SetValue_ShouldNotCrash_And_StayConsistent()
            {
                var storage = CreateStorage();
                var exceptions = new List<Exception>();

                Parallel.For(0, ThreadCount, threadId =>
                {
                    try
                    {
                        for (int i = 0; i < ElementCount; i++)
                        {
                            // Đọc và ghi lại giá trị (dễ sinh race)
                            var raw = storage.GetValue(i);
                            var current = raw is null ? 0 : (long)raw;
                            storage.SetValue(i, current + 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                            exceptions.Add(ex);
                    }
                });

                // Assert: Không có exception nào
                Assert.Empty(exceptions);

                // Assert: Tổng giá trị ở mỗi ô phải bằng ThreadCount nếu không race
                for (int i = 0; i < ElementCount; i++)
                {
                    var result = storage.GetValue(i);
                    Assert.True(result is long, $"Value at index {i} is null");
                    Assert.Equal(ThreadCount, (long)result!);
                }
            }
        }
    }
}
