using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.ValueStorage;

namespace test
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

            Assert.Equal(typeof(long), Support.InferDataType(list));
            AbstractValueStorage storage = Series.ValueStorageCreate(list);
            Assert.True(storage is IntValuesStorage); // your internal naming
        }

        public static IEnumerable<object[]> TypeInferenceData => new List<object[]>
        {
            new object[] { new object[] { 1, 2, 3 }, typeof(long), typeof(IntValuesStorage) },
            new object[] { new object[] { 1.0f, 2.0f }, typeof(double), typeof(DoubleValueStorage) },
            new object[] { new object[] { new DateTime(2020, 1, 1) }, typeof(DateTime), typeof(DateTimeStorage) },
            new object[] { new object[] { Guid.NewGuid() }, typeof(Guid), typeof(ObjectValueStorage) },
        };

        [Theory]
        [MemberData(nameof(TypeInferenceData))]
        public void TestTypeInfer_Multiple(object[] rawData, Type expectedType, Type expectedStorageType)
        {
            var list = rawData.ToList();
            var inferred = Support.InferDataType(list);
            Assert.Equal(expectedType, inferred);

            var storage = Series.ValueStorageCreate(list);
            Assert.Equal(expectedStorageType, storage.GetType());
        }
    }
}
