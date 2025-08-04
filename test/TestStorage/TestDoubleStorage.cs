using DataProcessor.source.ValueStorage;
namespace test.TestStorage
{
    public class TestDoubleStorage
    {

        [Fact]
        public void TestNullHandlingInDoubleStorage()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { null, 3.5, null });
            Assert.Equal(3, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 0, 2 }));
            Assert.Null(doubleStorage.GetValue(0));
            Assert.Equal(3.5, doubleStorage.GetValue(1));
        }

        [Fact]
        public void TestDoubleStorageWithAllNulls()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { null, null, null });
            Assert.Equal(3, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 0, 1, 2 }));
            Assert.Null(doubleStorage.GetValue(0));
            Assert.Null(doubleStorage.GetValue(1));
            Assert.Null(doubleStorage.GetValue(2));
        }

        public void TestDoubleStorageWithEmptyArray()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { });
            Assert.Equal(0, doubleStorage.Count);
            Assert.Empty(doubleStorage.NullIndices);
        }

        [Fact]
        public void TestDoubleStorageWithSingleNullValue()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { null });
            Assert.Equal(1, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 0 }));
            Assert.Null(doubleStorage.GetValue(0));
        }
        [Fact]
        public void TestDoubleStorageWithSingleValue()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { 5.0 });
            Assert.Equal(1, doubleStorage.Count);
            Assert.Empty(doubleStorage.NullIndices);
            Assert.Equal(5.0, doubleStorage.GetValue(0));
        }

        [Fact]
        public void TestDoubleStorageWithMixedValues()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { 1.0, null, 3.5, null, 5.0 });
            Assert.Equal(5, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 1, 3 }));
            Assert.Equal(1.0, doubleStorage.GetValue(0));
            Assert.Null(doubleStorage.GetValue(1));
            Assert.Equal(3.5, doubleStorage.GetValue(2));
            Assert.Null(doubleStorage.GetValue(3));
            Assert.Equal(5.0, doubleStorage.GetValue(4));
        }

        [Fact]
        public void TestDoubleStorageWithNegativeValues()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { -1.0, -2.5, null, -4.0 });
            Assert.Equal(4, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 2 }));
            Assert.Equal(-1.0, doubleStorage.GetValue(0));
            Assert.Equal(-2.5, doubleStorage.GetValue(1));
            Assert.Null(doubleStorage.GetValue(2));
            Assert.Equal(-4.0, doubleStorage.GetValue(3));
        }
        [Fact]
        public void TestDoubleStorageWithLargeValues()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { 1e10, 2e10, null, 4e10 });
            Assert.Equal(4, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 2 }));
            Assert.Equal(1e10, doubleStorage.GetValue(0));
            Assert.Equal(2e10, doubleStorage.GetValue(1));
            Assert.Null(doubleStorage.GetValue(2));
            Assert.Equal(4e10, doubleStorage.GetValue(3));
        }
        [Fact]
        public void TestDoubleStorageWithNaNAndInfinity()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity });
            Assert.Equal(3, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new int[] { })); // No nulls
            Assert.True(double.IsNaN((double)doubleStorage.GetValue(0)));
            Assert.Equal(double.PositiveInfinity, doubleStorage.GetValue(1));
            Assert.Equal(double.NegativeInfinity, doubleStorage.GetValue(2));
        }

        [Fact]
        public void TestDoubleStorageWithNullsAndSpecialValues()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { null, double.NaN, double.PositiveInfinity, null, double.NegativeInfinity });
            Assert.Equal(5, doubleStorage.Count);
            Assert.True(doubleStorage.NullIndices.SequenceEqual(new[] { 0, 3 }));
            Assert.Null(doubleStorage.GetValue(0));
            Assert.True(double.IsNaN((double)doubleStorage.GetValue(1)));
            Assert.Equal(double.PositiveInfinity, doubleStorage.GetValue(2));
            Assert.Null(doubleStorage.GetValue(3));
            Assert.Equal(double.NegativeInfinity, doubleStorage.GetValue(4));
        }

        [Fact]
        public void TestDoubleStorageSetValue()
        {
            var doubleStorage = new DoubleValueStorage(new double?[] { null, null });
            doubleStorage.SetValue(0, 1.5);
            doubleStorage.SetValue(1, 2.5);
            Assert.Equal(1.5, doubleStorage.GetValue(0));
            Assert.Equal(2.5, doubleStorage.GetValue(1));
            Assert.Throws<ArgumentException>(() => doubleStorage.SetValue(0, "Not a double")); // Invalid type
            Assert.Throws<ArgumentOutOfRangeException>(() => doubleStorage.GetValue(2)); // Accessing out of bounds index
        }

        [Fact]
        public void TestDoubleStorageValueProperty()
        {
            DoubleValueStorage doubleStorage = new DoubleValueStorage(new double?[] { 1.0, null, 3.5 });
            var values = doubleStorage.Values.ToList();
            Assert.Equal(2, values.Count);
            Assert.Equal(1.0, values[0]);
            Assert.Equal(3.5, values[1]);
        }

        [Fact]
        public void TestDoubleStorageWithLinq()
        {

            var doubleStorage = new DoubleValueStorage(new double?[] { 3.14, 2.71, null });
            var result = doubleStorage.ToList().AsReadOnly();
            Assert.Equal(3, result.Count);
            Assert.Equal(3.14, result[0]);
            Assert.Equal(2.71, result[1]);
            Assert.Null(result[2]);
        }
    }
}
