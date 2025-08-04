using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.UserSettings;
namespace test.TestNonGenericsSeries
{
    public class TestConstructor
    {
        [Theory]
        [InlineData(
            new object[] { 1, null, "a" },
            new object[] { "x", "y", "z" }
        )]
        [InlineData(
            new object[] { 3.14, 2.71, null },
            new object[] { 0, 1, 2 }
        )]
        [InlineData(
            new object[] { null, "abc", 123 },
            new object[] { "a", "b", "c" }
        )]
        [InlineData(
            new object[] { typeof(string), null },
            new object[] { "a", "b", "c" }
        )]
        [InlineData(
            new object[] { 1.1, 2.2, null, 3.3 },
            new object[] { "a", "b", "c", "d" }
        )]
        [InlineData(
            new object[] { 1.1, 2.2, null, 3.3 },
            new object[] { 0, 1, 2, 3 }
        )]
        public void TestConstructorWithValuesAndIndex(object[] values, object[] index)
        {
            // Arrange
            var series = new Series(values, index);
            // Act
            var valuesList = series.Values.ToList();
            var indexList = series.Index.ToList();
            var tmp = valuesList.ToList();
            // Assert
            Assert.Equal(values.Length, valuesList.Count);
            Assert.Equal(index.Length, indexList.Count);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], valuesList[i]);
            }
            for (int i = 0; i < index.Length; i++)
            {
                Assert.Equal(index[i].ToString(), indexList[i].ToString());
            }
        }

        [Fact]
        public void TestNonGenericsWithUserSpecificType1()
        {
            var values = new object[] { 1, 2, 3, 4, 5, 6.7, null };
            var series = new Series(values, dtype: typeof(string));
            var valuesList = series.Values.ToList();
            for (int i = 0; i < valuesList.Count; i++)
            {
                if (valuesList[i] == null)
                    Assert.Null(series.GetValueIntloc(i));
                else
                    Assert.Equal(values[i].ToString(), series.GetValueIntloc(i).ToString());
            }
            Assert.Equal(typeof(string), series.DataType);
        }

        [Fact]
        public void TestNonGenericsWithUserSpecificType2()
        {
            var values = new object?[] { 1, 2, 3, 4, 5, 6.7, null };
            var series = new Series(values, dtype: typeof(object));
            var valuesList = series.Values.ToList();
            for (int i = 0; i < valuesList.Count; i++)
            {
                if (valuesList[i] == null)
                    Assert.Null(series.GetValueIntloc(i));
                else
                    Assert.Equal(values[i].ToString(), series.GetValueIntloc(i).ToString());
            }
            Assert.Equal(typeof(object), series.DataType);
        }

        [Fact]
        public void TestInvalidCastException()
        {
            var values = new object?[] { 1, 2, 3, 4, 5, 6.7, "abc", null };
            Assert.Throws<InvalidCastException>(() => new Series(values, dtype: typeof(int)));
        }

        [Fact]
        public void Series_IndexValue_MatchOneToOne1()
        {
            var index = new object?[] { 1, 2, 3, 4, 5, 78 };
            var values = new object?[] { 1, null, 2, 3, null, 4 };

            var series = new Series(values, index, copy: false);

            for (int i = 0; i < index.Length; i++)
            {
                List<object?> items = series[index[i]];
                Assert.Single(items); // rõ nghĩa hơn Count == 1
                if (values[i] == null)
                {
                    Assert.Null(items[0]); // because items has only 1 element
                }
                else
                {
                    Assert.Equal(Convert.ToInt64(values[i]), Convert.ToInt64(items[0]));
                }
            }
        }

        [Fact]
        public void Series_IndexValue_MatchOneTOOne2()
        {
            UserConfig.DefaultNormalizationForm = System.Text.NormalizationForm.FormD;
            // Purposefully strange index to verify robustness of indexing logic.
            var index = new object?[] { "anh yêu em nhiều", "anh yêu C#", "búa liềm thần chưởng" };
            var values = new object?[] { "nghiệp chướng", null, 99 };
            var series = new Series(values, index, copy: false);
            Assert.Null((series["anh yêu C#"][0]));
            Assert.Equal("nghiệp chướng", series["anh yêu em nhiều"][0]);
            Assert.Equal(99, Convert.ToInt64(series["búa liềm thần chưởng"][0]));
            UserConfig.DefaultNormalizationForm = System.Text.NormalizationForm.FormC;
        }
        [Fact]
        public void Series_IndexValue_MatchManyToMany()
        {
            // set up data for test
            var index = new List<object> { "foo", "bar", "foo", "baz", "bar" };
            var data = new List<int> { 10, 20, 30, 40, 50 };
            var series = new Series(data, index);

            Dictionary<object, List<int>> mapping = new Dictionary<object, List<int>>();
            for (int i = 0; i < index.Count; i++)
            {
                if (!mapping.ContainsKey(index[i])) { mapping[index[i]] = new List<int>(); }
                mapping[index[i]].Add(i);
            }

            // action
            for (int i = 0; i < index.Count; i++)
            {
                var result = series[index[i]];
                Assert.Equal(mapping[index[i]].Count, result.Count);
                var positions = mapping[index[i]];
                foreach (var pos in positions)
                {
                    Assert.Contains(data[pos], result.Cast<long>().ToList());
                }
            }
        }
    }
}
