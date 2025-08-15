using DataProcessor.source.NonGenericsSeries;
namespace test.TestNonGenericsSeries
{
    public class TesrPropertiesAndComponent
    {
        [Theory]
        [InlineData(
            new object[] { 1, null, "a" },
            typeof(object),
            new object[] { "x", "y", "z" }
        )]
        [InlineData(
            new object[] { 3.14, 2.71, null },
            typeof(double),
            new object[] { 0, 1, 2 }
        )]
        [InlineData(
            new object[] { null, "abc", 123 },
            typeof(object),
            new object[] { "a", "b", "c" }
        )]
        [InlineData(
            new object[] { 1.1, 2.2, null, 3.3 },
            typeof(double),
            new object[] { "a", "b", "c", "d" }
        )]
        [InlineData(
            new object[] { 1.1, 2.2, null, 3.3 },
            typeof(double),
            new object[] { 0, 1, 2, 3 }
        )]
        public void DataTypeProperties(object[] values, Type expectedType, object[] index)
        {
            var series = new Series(values, index);
            Assert.Equal(expectedType, series.DataType);
        }

        [Fact]
        public void TestEnumeratorWithInteger()
        {
            var values = new List<int>();
            var random = new Random();
            for (int i = 0; i < 100000; i++)
                values.Add(random.Next());

            var series = new Series(values);

            Action act = () =>
            {
                int max = -9999999;
                foreach (var element in series.AsTyped<int>())
                    if (element > max)
                        max = element;
            };

            var ex = Record.Exception(act);
            Assert.Null(ex); // Null nghĩa là không có exception
        }
        [Fact]
        public void TestEnumeratorWithDouble()
        {
            var values = new List<double>();
            var random = new Random();
            for (int i = 0; i < 100000; i++)
            {
                values.Add(random.NextDouble());
            }

            var series = new Series(values, copy: true);
            // find max in values
            double maxInData = double.MinValue;
            foreach (var element in values)
            {
                if (element > maxInData)
                    maxInData = element;
            }

            double maxInSeries = double.MinValue;
            foreach (var element in series.AsTyped<double>())
            {
                if (element > maxInSeries)
                    maxInSeries = element;
            }
            Assert.Equal(maxInData, maxInSeries);
        }
    }
}
