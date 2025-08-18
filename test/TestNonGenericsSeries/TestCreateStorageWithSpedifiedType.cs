using DataProcessor.source.ValueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.NonGenericsSeries;
namespace test.TestNonGenericsSeries
{
    public class TestCreateStorageWithSpedifiedType
    {
       
        public static IEnumerable<object[]> TypeInferenceData => new List<object[]>
        {
            new object[] { new object[] { 1, 2, 3 }, typeof(long), typeof(Int64ValuesStorage) },
            new object[] { new object[] { 1.0f, 2.0f }, typeof(double), typeof(DoubleValueStorage) },
            new object[] { new object[] { new DateTime(2020, 1, 1) }, typeof(DateTime), typeof(DateTimeStorage) },
            new object[] { new object[] { Guid.NewGuid() }, typeof(Guid), typeof(ObjectValueStorage) },
            new object[] { new object[] { "Hello", "World" }, typeof(string), typeof(StringStorage) },
            new object[] { new object[] { null, true, false }, typeof(bool), typeof(BoolStorage) },
            new object[] { new object[] { 3.14, 2.71, null }, typeof(double), typeof(DoubleValueStorage) },
            new object[] { new object[] { new Guid() }, typeof(Guid), typeof(ObjectValueStorage)}
        };
        [Theory]
        [MemberData(nameof(TypeInferenceData))]

        public void RunTest(object[] rawData, Type type, Type expectedTypeStorage)
        {
            Assert.Equal(expectedTypeStorage, Series.CreateValueStorage(type, rawData.ToList(), false).GetType());
        }
    }
}
