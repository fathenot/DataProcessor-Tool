namespace test.TestStorage
{
    public class TestStringStorage
    {
        [Fact]
        public void TestStringStorageWithNulls()
        {
            var stringStorage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { null, "hello", null, "world" });
            Assert.Equal(4, stringStorage.Count);
            Assert.True(stringStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(stringStorage.GetValue(0));
            Assert.Equal("hello", stringStorage.GetValue(1));
            Assert.Null(stringStorage.GetValue(2));
            Assert.Equal("world", stringStorage.GetValue(3));
        }

        [Fact]
        public void TestStringStorageWithAllNulls()
        {
            var stringStorage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { null, null, null });
            Assert.Equal(3, stringStorage.Count);
            Assert.True(stringStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(stringStorage.GetValue(0));
            Assert.Null(stringStorage.GetValue(1));
            Assert.Null(stringStorage.GetValue(2));
        }

        [Fact]
        public void TestStringStorageWithEmptyArray()
        {
            var stringStorage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { });
            Assert.Equal(0, stringStorage.Count);
            Assert.Empty(stringStorage.NullIndices);
        }

        [Fact]
        public void TestUnicodeStringStorage()
        {
            var stringStorage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "こんにちは", "世界" });
            Assert.Equal(2, stringStorage.Count);
            Assert.Empty(stringStorage.NullIndices);
            Assert.Equal("こんにちは", stringStorage.GetValue(0));
            Assert.Equal("世界", stringStorage.GetValue(1));
        }

        [Fact]
        public void TestUnicodeStringStorageWithNulls()
        {
            var stringStorage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { null, "こんにちは", null, "世界" });
            Assert.Equal(4, stringStorage.Count);
            Assert.True(stringStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(stringStorage.GetValue(0));
            Assert.Equal("こんにちは", stringStorage.GetValue(1));
            Assert.Null(stringStorage.GetValue(2));
            Assert.Equal("世界", stringStorage.GetValue(3));
        }

        [Fact]
        public void TestStringStorage_ComplexUnicode()
        {
            // Chuỗi chứa emoji, các ngôn ngữ khác nhau và null
            var inputs = new string?[]
            {
            null,
            "😀", // emoji U+1F600
            "💩", // emoji U+1F4A9
            "A\u030A", // A với vòng tròn nhỏ phía trên (U+030A) - tổ hợp
            "Å",       // A có vòng tròn sẵn (U+00C5) - composed
            "مرحبا",    // Arabic
            "שלום",     // Hebrew
            "សួស្តី",   // Khmer
            null,
            "வணக்கம்"  // Tamil
            };

            var expectedNonNull = new string?[]
            {
                "😀", "💩", "A\u030A", "Å", "مرحبا", "שלום", "សួស្តី", "வணக்கம்"
            };

            var storage = new DataProcessor.source.ValueStorage.StringStorage(inputs, false);

            // Kiểm tra count
            Assert.Equal(inputs.Length, storage.Count);

            // Kiểm tra chỉ số null
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 0, 8 }));
            // Kiểm tra từng phần tử
            for (int i = 0; i < inputs.Length; i++)
            {
                Assert.Equal(inputs[i], (string)storage.GetValue(i));
            }

            // Kiểm tra enumerator hoạt động chính xác
            var fromEnum = storage.ToList();
            Assert.True(fromEnum.SequenceEqual(inputs));
        }

        [Fact]
        public void TestStringStorage_SetValue_ValidUpdates()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "hello", null, "世界" });

            // Ghi đè vị trí có null thành emoji
            storage.SetValue(1, "🔥");

            // Ghi đè chuỗi thường thành tổ hợp Unicode
            storage.SetValue(0, "A\u030A"); // A + ring

            // Kiểm tra lại giá trị sau cập nhật
            Assert.Equal("A\u030A", storage.GetValue(0));
            Assert.Equal("🔥", storage.GetValue(1));
            Assert.Equal("世界", storage.GetValue(2));

            // Kiểm tra null index cập nhật đúng
            Assert.True(storage.NullIndices.SequenceEqual(new int[0]));
        }

        [Fact]
        public void TestStringStorage_SetValue_InvalidType_Throws()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "a", "b", "c" });

            // Dùng số nguyên, sẽ ném lỗi
            Assert.Throws<ArgumentException>(() => storage.SetValue(0, 42));

            // Dùng object không phải string/null
            Assert.Throws<ArgumentException>(() => storage.SetValue(1, new object()));
        }

        [Fact]
        public void TestStringStorage_SetValue_ToNull()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "x", "y", "z" });

            storage.SetValue(1, null);

            Assert.Null(storage.GetValue(1));
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 1 }));
        }

        [Fact]
        public void TestStringStorage_ApplyLinq()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "apple", "banana", null, "cherry" });
            var result = storage.AsTyped<string?>().Where(x => x != null).Select(x => x!.ToUpper()).ToList();
            Assert.Equal(3, result.Count);
            Assert.Contains("APPLE", result);
            Assert.Contains("BANANA", result);
            Assert.Contains("CHERRY", result);
        }

        [Fact]
        public void TestCultureInvariantNormalization()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "A\u030A", "Å" });
            Assert.Equal("A\u030A", storage.GetValue(0));
            Assert.Equal("Å", storage.GetValue(1));
            // Kiểm tra xem chúng có được coi là bằng nhau không
            Assert.Equal(storage.GetValue(0), storage.GetValue(1));
        }

        [Fact]
        public void TestStringStorage_NullHandling()
        {
            var storage = new DataProcessor.source.ValueStorage.StringStorage(new string?[] { "test", null, "data" });
            Assert.Equal(3, storage.Count);
            Assert.True(storage.NullIndices.SequenceEqual(new[] { 1 }));
            Assert.Null(storage.GetValue(1));
            storage.SetValue(1, "new value");
            Assert.Equal("new value", storage.GetValue(1));
            Assert.True(storage.NullIndices.SequenceEqual(new int[0]));

            var nonNullVals = storage.Values;
            Assert.Equal(3, nonNullVals.Count());
            Assert.Contains("test", nonNullVals);
            Assert.Contains("data", nonNullVals);
        }
    }
}
