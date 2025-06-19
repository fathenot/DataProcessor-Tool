using DataProcessor.source.ValueStorage;

namespace test
{
    public class TestObjectStorage
    {
        [Fact]
        public void TestObjectStorageWithNulls()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { null, "hello", null, 42 });
            Assert.Equal(4, objectStorage.Count);
            Assert.True(objectStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(objectStorage.GetValue(0));
            Assert.Equal("hello", objectStorage.GetValue(1));
            Assert.Null(objectStorage.GetValue(2));
            Assert.Equal(42, objectStorage.GetValue(3));
        }

        [Fact]
        public void TestObjectStorageWithAllNulls()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { null, null, null });
            Assert.Equal(3, objectStorage.Count);
            Assert.True(objectStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(objectStorage.GetValue(0));
            Assert.Null(objectStorage.GetValue(1));
            Assert.Null(objectStorage.GetValue(2));
        }

        [Fact]
        public void TestObjectStorageWithEmptyArray()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { });
            Assert.Equal(0, objectStorage.Count);
            Assert.Empty(objectStorage.NullIndices);
        }

        [Fact]
        public void TestObjectStorageWithMixedValues()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { "test", null, 123, null, true });
            Assert.Equal(5, objectStorage.Count);
            Assert.True(objectStorage.NullIndices.SequenceEqual(new[] { 1, 3 }));
            Assert.Equal("test", objectStorage.GetValue(0));
            Assert.Null(objectStorage.GetValue(1));
            Assert.Equal(123, objectStorage.GetValue(2));
            Assert.Null(objectStorage.GetValue(3));
            Assert.Equal(true, objectStorage.GetValue(4));
        }

        [Fact]
        public void TestApplyLinq()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { "apple", "banana", null, "cherry" });
            var result = objectStorage.Where(x => x != null).Select(x => x.ToString().ToUpper()).ToList();
            Assert.Equal(3, result.Count);
            Assert.Contains("APPLE", result);
            Assert.Contains("BANANA", result);
            Assert.Contains("CHERRY", result);
        }

        [Fact]
        public void TesEnumerator()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { "one", "two", "three" });
            var enumerator = objectStorage.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal("one", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("two", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("three", enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }


        [Fact]
        public void TestSetValue()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { null, null });
            objectStorage.SetValue(0, "new value");
            objectStorage.SetValue(1, 123);
            Assert.Equal("new value", objectStorage.GetValue(0));
            Assert.Equal(123, objectStorage.GetValue(1));
        }

        [Fact]
        public void TestElementType()
        {
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { "test", null, 42 });
            Assert.Equal(typeof(object), objectStorage.ElementType);
        }

        [Fact]
        public void TestStorage_WithUserDefinedTypes()
        {
            var customObject1 = new { Name = "Alice", Age = 30 };
            var customObject2 = new { Name = "Bob", Age = 25 };
            var objectStorage = new DataProcessor.source.ValueStorage.ObjectValueStorage(new object?[] { customObject1, null, customObject2 });

            Assert.Equal(3, objectStorage.Count);
            Assert.True(objectStorage.NullIndices.SequenceEqual(new[] { 1 }));
            Assert.Equal(customObject1, objectStorage.GetValue(0));
            Assert.Null(objectStorage.GetValue(1));
            Assert.Equal(customObject2, objectStorage.GetValue(2));
        }

        [Fact]
        public void TestObjectValueStorage_WithWeirdObjects()
        {
            var obj1 = new { Name = "Alice", Nested = new { Age = 30, Langs = new[] { "C#", "F#" } } };
            var obj2 = new { Id = 123, Timestamp = DateTime.Now, Guid = Guid.NewGuid() };
            var obj3 = new { Symbol = "🔥", Count = 99 };

            var storage = new ObjectValueStorage(new object?[] { obj1, null, obj2, obj3 });

            Assert.Equal(4, storage.Count);
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 1 }));

            Assert.Equal(obj1, storage.GetValue(0));
            Assert.Null(storage.GetValue(1));
            Assert.Equal(obj2, storage.GetValue(2));
            Assert.Equal(obj3, storage.GetValue(3));
        }

        [Fact]
        public void TestObjectValueStorage_NestedAnonymousObjects()
        {
            var nestedObj = new
            {
                User = new
                {
                    Id = 1,
                    Name = "ユーザー",
                    Attributes = new[] { "fast", "🔥", "日本語" }
                }
            };

            var storage = new ObjectValueStorage(new object?[] { nestedObj, null });

            Assert.Equal(2, storage.Count);
            Assert.Null(storage.GetValue(1));
            Assert.Equal(nestedObj, storage.GetValue(0));
        }

        [Fact]
        public void TestObjectValueStorage_SpecialTypes()
        {
            var special = new
            {
                Timestamp = DateTime.UtcNow,
                SessionId = Guid.NewGuid(),
                Amount = 199.99m
            };

            var storage = new ObjectValueStorage(new object?[] { special });

            Assert.Equal(special, storage.GetValue(0));
        }

        [Fact]
        public void TestObjectValueStorage_SetValueAndMutation()
        {
            var original = new { Name = "Before", Age = 10 };
            var updated = new { Name = "After", Age = 20 };

            var storage = new ObjectValueStorage(new object?[] { original, null });

            storage.SetValue(1, updated);

            Assert.Equal(original, storage.GetValue(0));
            Assert.Equal(updated, storage.GetValue(1));
        }

        class AlwaysEqual
        {
            public string Data { get; set; } = "";

            public override bool Equals(object? obj) => true;
            public override int GetHashCode() => 1;
        }

        [Fact]
        public void TestObjectValueStorage_WithAlwaysEqualClass()
        {
            var a = new AlwaysEqual { Data = "A" };
            var b = new AlwaysEqual { Data = "B" };

            var storage = new ObjectValueStorage(new object?[] { a, b });

            Assert.NotSame(a, storage.GetValue(0));
            Assert.NotSame(b, storage.GetValue(1));
        }

        [Fact]
        public void TestObjectValueStorage_SetToNull()
        {
            var a = new { Id = 1 };
            var storage = new ObjectValueStorage(new object?[] { a });

            storage.SetValue(0, null);

            Assert.Null(storage.GetValue(0));
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 0 }));
        }

        [Fact]
        public void RunAllTests()
        {
            TestObjectStorageWithNulls();
            TestObjectStorageWithAllNulls();
            TestObjectStorageWithEmptyArray();
            TestObjectStorageWithMixedValues();
            TestApplyLinq();
            TesEnumerator();
            TestSetValue();
            TestElementType();
            TestStorage_WithUserDefinedTypes();
            TestObjectValueStorage_WithWeirdObjects();
            TestObjectValueStorage_NestedAnonymousObjects();
            TestObjectValueStorage_SpecialTypes();
            TestObjectValueStorage_SetValueAndMutation();
            TestObjectValueStorage_WithAlwaysEqualClass();
            TestObjectValueStorage_SetToNull();
        }
    }
}
