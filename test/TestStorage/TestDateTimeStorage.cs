using DataProcessor.source.ValueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.TestStorage
{
    public class TestDateTimeStorage
    {
        [Fact]
        public void TestDateTimeStorageWithNulls()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { null, DateTime.Now, null, DateTime.UtcNow });
            Assert.Equal(4, dateTimeStorage.Count);
            Assert.True(dateTimeStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(dateTimeStorage.GetValue(0));
            Assert.NotNull(dateTimeStorage.GetValue(1));
            Assert.Null(dateTimeStorage.GetValue(2));
            Assert.NotNull(dateTimeStorage.GetValue(3));
        }
        [Fact]
        public void TestDateTimeStorageWithAllNulls()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { null, null, null });
            Assert.Equal(3, dateTimeStorage.Count);
            Assert.True(dateTimeStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(dateTimeStorage.GetValue(0));
            Assert.Null(dateTimeStorage.GetValue(1));
            Assert.Null(dateTimeStorage.GetValue(2));
        }

        [Fact]
        public void TestDateTimeStorageWithEmptyArray()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { });
            Assert.Equal(0, dateTimeStorage.Count);
            Assert.Empty(dateTimeStorage.NullIndices);
        }

        [Fact]
        public void TestDateTimeStorageSetValue()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { null, null });
            dateTimeStorage.SetValue(0, DateTime.Now);
            dateTimeStorage.SetValue(1, DateTime.UtcNow);

            Assert.NotNull(dateTimeStorage.GetValue(0));
            Assert.NotNull(dateTimeStorage.GetValue(1));
            Assert.Throws<ArgumentException>(() => dateTimeStorage.SetValue(0, "Not a DateTime"));
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTimeStorage.GetValue(2)); // Accessing out of bounds index
        }

        [Fact]
        public void TestApplyLinq()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { DateTime.Now, null, DateTime.UtcNow });
            var result = dateTimeStorage.AsTyped<DateTime?>().Where(dt => dt.HasValue && dt.Value.Kind == DateTimeKind.Utc).ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal(DateTimeKind.Utc, result[0].Value.Kind);
        }

        [Fact]
        public void TestEnumerator()
        {
            var dateTimeStorage = new DateTimeStorage(new DateTime?[] { DateTime.Now, null, DateTime.UtcNow });
            int count = 0;
            List<DateTime?> dateTimeList = new List<DateTime?>();
            foreach (var value in dateTimeStorage)
            {
                if (value is DateTime dt)
                {
                    Assert.True(dt.Kind == DateTimeKind.Utc || dt.Kind == DateTimeKind.Local);
                }
                dateTimeList.Add(value as DateTime?);
            }
            Assert.Contains(null, dateTimeList); // Ensure nulls are included
            Assert.Equal(3, dateTimeList.Count);
            var asTyped = dateTimeStorage.AsTyped<DateTime?>().ToList();
            Assert.Equal(3, asTyped.Count); // Ensure typed enumeration matches original count
            Assert.Contains(null, asTyped); // Ensure nulls are included in typed enumeration
        }

        [Fact]
        public void NativePointer_ShouldBeNonZero()
        {
            var storage = new DateTimeStorage(new DateTime?[] { DateTime.UtcNow });
            var ptr = storage.GetNativeBufferPointer();
            Assert.NotEqual(nint.Zero, ptr);
        }

        [Fact]
        public void SetValue_ShouldAccept_Convertible()
        {
            var dt = DateTime.UtcNow;
            var storage = new DateTimeStorage(new DateTime?[] { null });
            storage.SetValue(0, dt.ToString("o"));
            Assert.Equal(dt.ToString("o"), ((DateTime)storage.GetValue(0)).ToString("o"));
            Assert.Equal(dt.Kind, ((DateTime)storage.GetValue(0)).Kind);
        }

    }
}
