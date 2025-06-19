namespace test
{
    public class TestGenericsStorage
    {
        [Fact]
        public void TestGenericsStorageWithNulls()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { null, "hello", null, "world" });
            Assert.Equal(4, storage.Count);
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(storage.GetValue(0));
            Assert.Equal("hello", storage.GetValue(1));
            Assert.Null(storage.GetValue(2));
            Assert.Equal("world", storage.GetValue(3));
        }
        [Fact]
        public void TestGenericsStorageWithAllNulls()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { null, null, null });
            Assert.Equal(3, storage.Count);
            Assert.True(storage.NullIndices.SequenceEqual([0, 1, 2]));
            Assert.Null(storage.GetValue(0));
            Assert.Null(storage.GetValue(1));
            Assert.Null(storage.GetValue(2));
        }
        [Fact]
        public void TestGenericsStorageWithEmptyArray()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { });
            Assert.Equal(0, storage.Count);
            Assert.Empty(storage.NullIndices);
        }
        [Fact]
        public void TestGenericsStorageWithMixedValues()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { "test", null, "123", null, "true" });
            Assert.Equal(5, storage.Count);
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 1, 3 }));
            Assert.Equal("test", storage.GetValue(0));
            Assert.Null(storage.GetValue(1));
            Assert.Equal("123", storage.GetValue(2));
            Assert.Null(storage.GetValue(3));
            Assert.Equal("true", storage.GetValue(4));
        }
        [Fact]
        public void TestGenericsStorageSetValue()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { null, null });
            storage.SetValue(0, "first");
            storage.SetValue(1, "second");
            Assert.Equal("first", storage.GetValue(0));
            Assert.Equal("second", storage.GetValue(1));
            Assert.Throws<ArgumentException>(() => storage.SetValue(0, 123)); // Invalid type
            Assert.Throws<ArgumentOutOfRangeException>(() => storage.GetValue(2)); // Accessing out of bounds index
        }

        [Fact]
        public void TestApplyLinqOnGenericsStorage()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { "apple", "banana", null, "cherry" });
            var result = storage.Where(x => x != null).Select(x => x!.ToUpper()).ToList();
            Assert.Equal(3, result.Count);
            Assert.Contains("APPLE", result);
            Assert.Contains("BANANA", result);
            Assert.Contains("CHERRY", result);
        }

        [Fact]
        public void TestEnumeratorOnGenericsStorage()
        {
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<string?>(new string?[] { "one", "two", null, "three" });
            var enumeratedValues = storage.ToList();
            Assert.Equal(4, enumeratedValues.Count);
            Assert.Equal("one", enumeratedValues[0]);
            Assert.Equal("two", enumeratedValues[1]);
            Assert.Null(enumeratedValues[2]);
            Assert.Equal("three", enumeratedValues[3]);
        }

        struct Point
        {
            public long x;
            public long y;
        }


        [Fact]

        public void TestGenericsStorageWithUserDefinedType()
        {
            var points = new Point[]
            {
                new Point { x = 1, y = 2 },
                new Point { x = 3, y = 4 },
                new Point { x = 5, y = 6 }
            };
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<Point>(points);
            Assert.Equal(3, storage.Count);
            Assert.Equal(points[0], storage.GetValue(0));
            Assert.Equal(points[1], storage.GetValue(1));
            Assert.Equal(points[2], storage.GetValue(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => storage.GetValue(3)); // Accessing out of bounds index

        }
        [Fact]
       public void TestGenericsStorageWithUserDefinedTypeAndNulls()
        {
            var points = new Point?[]
            {
                new Point { x = 1, y = 2 },
                null,
                new Point { x = 5, y = 6 }
            };
            var storage = new DataProcessor.source.ValueStorage.GenericsStorage<Point?>(points);
            Assert.Equal(3, storage.Count);
            Assert.Equal(points[0], storage.GetValue(0));
            Assert.Null(storage.GetValue(1));
            Assert.Equal(points[2], storage.GetValue(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => storage.GetValue(3)); // Accessing out of bounds index
        }

        [Fact]
        public void RunAllTests()
        {
            TestGenericsStorageWithNulls();
            TestGenericsStorageWithAllNulls();
            TestGenericsStorageWithEmptyArray();
            TestGenericsStorageWithMixedValues();
            TestGenericsStorageSetValue();
            TestApplyLinqOnGenericsStorage();
            TestEnumeratorOnGenericsStorage();
            TestGenericsStorageWithUserDefinedType();
            TestGenericsStorageWithUserDefinedTypeAndNulls();
        }
    }
}
