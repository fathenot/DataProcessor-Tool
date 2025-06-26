namespace test.TestStorage
{
    public class TestDecimalStorage
    {
        [Fact]
        public void TestDecimalStorageWithNulls()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { null, 3.5m, null, 4.2m });
            Assert.Equal(4, decimalStorage.Count);
            Assert.True(decimalStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(decimalStorage.GetValue(0));
            Assert.Equal(3.5m, decimalStorage.GetValue(1));
            Assert.Null(decimalStorage.GetValue(2));
            Assert.Equal(4.2m, decimalStorage.GetValue(3));
        }
        [Fact]
        public void TestDecimalStorageWithAllNulls()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { null, null, null });
            Assert.Equal(3, decimalStorage.Count);
            Assert.True(decimalStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(decimalStorage.GetValue(0));
            Assert.Null(decimalStorage.GetValue(1));
            Assert.Null(decimalStorage.GetValue(2));
        }
        [Fact]
        public void TestDecimalStorageWithEmptyArray()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { });
            Assert.Equal(0, decimalStorage.Count);
            Assert.Empty(decimalStorage.NullIndices);
        }

        [Fact]
        public void ApplyLinqToDecimalStorage()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { 1.1m, 2.2m, null, 3.3m });
            var result = decimalStorage.Where(x => x != null).Cast<decimal>().Select(x => x * 2).ToList();
            Assert.Equal(3, result.Count);
            Assert.Contains(2.2m, result);
            Assert.Contains(4.4m, result);
            Assert.Contains(6.6m, result);
        }

        [Fact]
        public void TestUpdateValue()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { 1.1m, 2.2m, null, 3.3m });
            decimalStorage.SetValue(1, 5.5m);
            Assert.Equal(5.5m, decimalStorage.GetValue(1));
        }

        [Fact]
        public void TestInvalidSetValue()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { 1.1m, 2.2m, null, 3.3m });
            Assert.Throws<ArgumentException>(() => decimalStorage.SetValue(1, "invalid value"));
            Assert.Throws<ArgumentOutOfRangeException>(() => decimalStorage.SetValue(5, 4.4m)); // Out of range index
        }

        [Fact]
        public void TestEnumerator()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { 1.1m, null, 3.3m });
            var enumerator = decimalStorage.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1.1m, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Null(enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(3.3m, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void TestAsTyped()
        {
            var decimalStorage = new DataProcessor.source.ValueStorage.DecimalStorage(new decimal?[] { 1.1m, 2.2m, null, 3.3m });
            var typedList = decimalStorage.AsTyped<decimal?>().ToList();
            Assert.Equal(4, typedList.Count);
            Assert.Equal(1.1m, typedList[0]);
            Assert.Equal(2.2m, typedList[1]);
            Assert.Null(typedList[2]);
            Assert.Equal(3.3m, typedList[3]);
        }

        [Fact]
        public void RunAllTests()
        {
            TestDecimalStorageWithNulls();
            TestDecimalStorageWithAllNulls();
            TestDecimalStorageWithEmptyArray();
            ApplyLinqToDecimalStorage();
            TestUpdateValue();
            TestInvalidSetValue();
            TestEnumerator();
            TestAsTyped();
        }
    }
}
