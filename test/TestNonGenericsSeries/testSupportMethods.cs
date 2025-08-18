using System;
using System.Collections.Generic;
using Xunit;
using DataProcessor.source.NonGenericsSeries;

namespace test.TestNonGenericsSeries
{
    public class testSupportMethods
    {
        [Theory]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(uint), true)]
        [InlineData(typeof(nint), true)]
        [InlineData(typeof(nuint), true)]
        [InlineData(typeof(ulong), false)]
        [InlineData(typeof(float), false)]
        [InlineData(typeof(string), false)]
        public void IsIntegerType_ReturnsExpectedResult(Type type, bool expected)
        {
            Assert.Equal(expected, Support.IsIntegerType(type));
        }

        [Theory]
        [InlineData(typeof(float), true)]
        [InlineData(typeof(double), true)]
        [InlineData(typeof(decimal), false)]
        [InlineData(typeof(int), false)]
        public void IsFloatingType_ReturnsExpectedResult(Type type, bool expected)
        {
            Assert.Equal(expected, Support.IsFloatingType(type));
        }

        [Theory]
        [InlineData(123, true)]
        [InlineData(123.456, true)]
        [InlineData(123.456f, true)]
        [InlineData("string", false)]
        [InlineData(null, false)]
        public void IsNumerics_ReturnsExpectedResult(object? value, bool expected)
        {
            Assert.Equal(expected, Support.IsNumerics(value));
        }

        [Fact]
        public void InferNumericType_ReturnsInt64_WhenOnlyIntegers()
        {
            var values = new List<object?> { 1, 2, 3, 4L };
            var result = Support.InferNumericType(values);
            Assert.Equal(typeof(long), result);
        }

        [Fact]
        public void InferNumericType_ReturnsInt32_WhenOnlyIntegers()
        {
            var values = new List<object?> { 1, 2, 3, 4 };
            var result = Support.InferNumericType(values);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void InferNumericType_ReturnsDouble_WhenFloatsPresent()
        {
            var values = new List<object?> { 1, 2.5f, 3 };
            var result = Support.InferNumericType(values);
            Assert.Equal(typeof(double), result);
        }

        [Fact]
        public void InferNumericType_ReturnsDecimal_WhenDecimalsPresent()
        {
            var values = new List<object?> { 1, (decimal)2.5, 3 };
            var result = Support.InferNumericType(values);
            Assert.Equal(typeof(decimal), result);
        }

        [Fact]
        public void InferNumericType_ReturnsObject_WhenMixedWithNonNumerics()
        {
            var values = new List<object?> { 1, "hello", 3 };
            var result = Support.InferNumericType(values);
            Assert.Equal(typeof(object), result);
        }

        [Fact]
        public void InferDataType_ReturnsObject_WhenOnlyNulls()
        {
            var values = new List<object?> { null, DBNull.Value };
            Assert.Equal(typeof(object), Support.InferDataType(values));
        }

        [Fact]
        public void InferDataType_ReturnsDecimal_WhenDecimalsPresent()
        {
            var values = new List<object?> { 1.5m, 2.5m, 3.5m };
            Assert.Equal(typeof(decimal), Support.InferDataType(values));
        }

        [Fact]
        public void InferDataType_ReturnsValueType_WhenDifferentStructs()
        {
            var values = new List<object?> { DateTime.Now, Guid.NewGuid() };
            Assert.Equal(typeof(ValueType), Support.InferDataType(values));
        }

        [Fact]
        public void InferDataType_ReturnsBaseClass_WhenReferenceTypesHaveCommonAncestor()
        {
            var values = new List<object?> { "abc", "def", "ghi" };
            Assert.Equal(typeof(string), Support.InferDataType(values));
        }

        [Fact]
        public void InferDataType_ReturnsObject_WhenMixedReferenceAndValueTypes()
        {
            var values = new List<object?> { "abc", 123 };
            Assert.Equal(typeof(object), Support.InferDataType(values));
        }

        [Theory]
        [InlineData(typeof(int), typeof(double), true)]
        [InlineData(typeof(double), typeof(int), true)]
        [InlineData(typeof(string), typeof(int), false)]
        [InlineData(typeof(long), typeof(object), true)]
        [InlineData(typeof(ulong), typeof(int), false)]
        [InlineData(typeof(int), typeof(ulong), false)]
        public void CanCastValueType_CheckCompatibility(Type from, Type to, bool expected)
        {
            var result = Support.CanCastValueType(from, to);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CanCastValueType_SupportsCustomOperator()
        {
            var result = Support.CanCastValueType(typeof(CustomA), typeof(CustomB));
            Assert.True(result);
        }

        private struct CustomA
        {
            public int Value;
            public static explicit operator CustomB(CustomA a) => new CustomB { Value = a.Value };
        }

        private struct CustomB
        {
            public int Value;
        }
    }
}
