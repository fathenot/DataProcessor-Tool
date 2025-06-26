using DataProcessor.source.NonGenericsSeries;
namespace test
{
    public class TestNonGenericsSeries
    {
        [Fact]
        public void TestNonGenericsSeriesCreation()
        {
            // Arrange
            List<object?> items = new List<object?> ();
            for (int i = 0; i < 100; i++)
            {
                items.Add(i);
            }
            var series = new Series(items);
            // Assert
            Assert.Equal(typeof(long),series.DataType);
            Assert.Equal(3, series.Count);
            Assert.Null(series[2]);
        }
    }
}
