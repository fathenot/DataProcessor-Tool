using DataProcessor.source.Index;
using DataProcessor.source.NonGenericsSeries;
namespace test.TestNonGenericsSeries
{
    public class TestCreateIndex
    {
        // This test method checks if the CreateIndex method correctly creates an index of the expected type
        // and contains the expected values from the provided indexValues array.
        public static IEnumerable<object[]> IndexTestCases =>
     new List<object[]>
     {
        new object[] { new object[] { 1, 2, 3 }, typeof(Int64Index) },
        new object[] { new object[] { "a", "b", "c" }, typeof(StringIndex) },
        new object[] { new object[] { 1.1, 2.2, 3.3 }, typeof(DoubleIndex) },
        new object[] { new object[] { DateTime.Parse("2020-01-01"), DateTime.Parse("2020-01-02") }, typeof(DateTimeIndex) },
        new object[] { new object[] { 'a', 'b' }, typeof(CharIndex) },
        new object[] { new object[] { 1.1m, 2.2m, 3.3m }, typeof(DecimalIndex) },
        new object[] { new object[] { "a", 1, 2.2, DateTime.Parse("2020-01-01") }, typeof(ObjectIndex) }
     };

        [Theory]
        [MemberData(nameof(IndexTestCases))]
        public void TestIndexCreation(object[] indexValues, Type expectedIndexType)
        {
            // Arrange

            var indexList = indexValues.Select(x => x).ToList();
            // Act
            var index = Series.CreateIndex(indexList);
            // Assert
            Assert.NotNull(index);
            Assert.IsType(expectedIndexType, index);
            Assert.Equal(indexValues.Length, index.Count);

            for (int i = 0; i < indexValues.Length; i++)
            {

                Assert.Equal(indexValues[i].ToString(), index[i].ToString());
            }
        }
    }

}
