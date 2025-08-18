using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.ValueStorage;

namespace test.TestNonGenericsSeries
{
    public class TestStorageWithTypeInfer
    {
        [Fact]
        public void TestTypeInfer_IntToLong()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i); // int
            }

            Assert.Equal(typeof(int), Support.InferDataType(list));
            AbstractValueStorage storage = Series.CreateValueStorage(list);
            Assert.True(storage is Int32ValuesStorage); // your internal naming
        }

        public static IEnumerable<object[]> TypeInferenceData => new List<object[]>
        {
            new object[] { new object[] { 1, 2, 3 }, typeof(int), typeof(Int32ValuesStorage) },
            new object[] { new object[] { 1.0f, 2.0f }, typeof(double), typeof(DoubleValueStorage) },
            new object[] { new object[] { new DateTime(2020, 1, 1) }, typeof(DateTime), typeof(DateTimeStorage) },
            new object[] { new object[] { Guid.NewGuid() }, typeof(Guid), typeof(ObjectValueStorage) },
            new object[] { new object[] { "Hello", "World" }, typeof(string), typeof(StringStorage) },
            new object[] { new object[] { null, true, false }, typeof(bool), typeof(BoolStorage) },
        };

        [Theory]
        [MemberData(nameof(TypeInferenceData))]
        public void TestTypeInfer_Multiple(object[] rawData, Type expectedType, Type expectedStorageType)
        {
            var list = rawData.ToList();
            var inferred = Support.InferDataType(list);
            Assert.Equal(expectedType, inferred);

            var storage = Series.CreateValueStorage(list);
            Assert.Equal(expectedStorageType, storage.GetType());
        }
    }
}
