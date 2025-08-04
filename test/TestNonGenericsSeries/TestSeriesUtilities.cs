using DataProcessor.source.NonGenericsSeries;

namespace test.TestNonGenericsSeries
{
    public class TestSeriesUtilities
    {
        [Fact]
        public void TestSorting()
        {
            List<int> data = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                data.Add(rnd.Next());
            }

           
            var series = new Series(data);
            data.Sort();
            series = series.SortValues();

            // test
            for (int i = 0; i < 100; i++)
            {
                Assert.True(series.Select(v => Convert.ToInt32(v)).ToList().SequenceEqual(data));
            }
        }
    }
}
