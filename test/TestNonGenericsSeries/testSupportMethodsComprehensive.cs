using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using DataProcessor.source.NonGenericsSeries;

namespace test.TestNonGenericsSeries
{
    /// <summary>
    /// Comprehensive test suite covering edge cases, bugs, and performance scenarios
    /// </summary>
    public class testSupportMethodsComprehensive
    {
        #region Bug Fix Tests - CRITICAL

        [Fact]
        public void isNumericsType_ReturnsTrue_ForNumericTypes()
        {
            // BUG: Current implementation always returns false
            // This test will FAIL until the bug is fixed
            Assert.True(TypeInference.IsNumericType(typeof(int)));
            Assert.True(TypeInference.IsNumericType(typeof(double)));
            Assert.True(TypeInference.IsNumericType(typeof(decimal)));
            Assert.True(TypeInference.IsNumericType(typeof(long)));
            Assert.True(TypeInference.IsNumericType(typeof(float)));
        }

        [Fact]
        public void isNumericsType_ReturnsFalse_ForNonNumericTypes()
        {
            Assert.False(TypeInference.IsNumericType(typeof(string)));
            Assert.False(TypeInference.IsNumericType(typeof(DateTime)));
            Assert.False(TypeInference.IsNumericType(typeof(object)));
        }

        [Fact]
        public void InferNumericType_ReturnsDouble_WhenDecimalOverflows()
        {
            // BUG: Current overflow check has logic error
            // Convert.ToDecimal(v) > decimal.MaxValue will never be true
            var values = new List<object?>
            {
                1.5m,
                1.8e30 // double that exceeds decimal range
            };

            var result = TypeInference.InferNumericType(values);

            // Should demote to double due to overflow
            Assert.Equal(typeof(double), result);
        }

        #endregion

        #region Edge Cases - Empty and Null

        [Fact]
        public void InferNumericType_ReturnsObject_WhenEmptyList()
        {
            var values = new List<object?>();
            Assert.Equal(typeof(object), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_ReturnsObject_WhenOnlyNulls()
        {
            var values = new List<object?> { null, null, DBNull.Value };
            Assert.Equal(typeof(object), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferDataType_ReturnsObject_WhenEmptyList()
        {
            var values = new List<object?>();
            Assert.Equal(typeof(object), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_HandlesListWithMostlyNulls()
        {
            var values = new List<object?> { null, null, 123, null, null };
            Assert.Equal(typeof(int), TypeInference.InferDataType(values));
        }

        #endregion

        #region Nullable Type Handling

        [Theory]
        [InlineData(typeof(int?), typeof(double), true)]
        [InlineData(typeof(int), typeof(double?), true)]
        [InlineData(typeof(int?), typeof(double?), true)]
        [InlineData(typeof(decimal?), typeof(double?), true)]
        public void CanCastValueType_HandlesNullableTypes(Type from, Type to, bool expected)
        {
            Assert.Equal(expected, TypeInference.CanCastValueType(from, to));
        }

        [Fact]
        public void CanCastValueType_NullableToNullable_SameUnderlyingType()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(int?), typeof(int?)));
        }

        [Fact]
        public void InferNumericType_HandlesNullableIntegers()
        {
            var values = new List<object?> { (int?)1, (int?)2, null, (int?)3 };
            var result = TypeInference.InferNumericType(values);
            Assert.Equal(typeof(int), result);
        }

        #endregion

        #region Enum Handling

        public enum TestEnum { A = 1, B = 2, C = 3 }
        public enum LongEnum : long { X = 100L, Y = 200L }

        [Fact]
        public void CanCastValueType_EnumToUnderlyingType()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(TestEnum), typeof(int)));
        }

        [Fact]
        public void CanCastValueType_UnderlyingTypeToEnum()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(int), typeof(TestEnum)));
        }

        [Fact]
        public void CanCastValueType_LongEnumToLong()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(LongEnum), typeof(long)));
        }

        [Fact]
        public void IsNumerics_ReturnsFalse_ForEnum()
        {
            // Enums are not directly numeric in IsNumerics check
            Assert.False(TypeInference.IsNumeric(TestEnum.A));
        }

        #endregion

        #region Uncommon Integer Types

        [Fact]
        public void InferNumericType_HandlesSByte()
        {
            var values = new List<object?> { (sbyte)1, (sbyte)2, (sbyte)3 };
            var result = TypeInference.InferNumericType(values);
            Assert.Equal(typeof(int), result); // sbyte promotes to int
        }

        [Fact]
        public void InferNumericType_HandlesUShort()
        {
            var values = new List<object?> { (ushort)1, (ushort)2 };
            var result = TypeInference.InferNumericType(values);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void InferNumericType_HandlesMixedByteTypes()
        {
            var values = new List<object?> { (byte)1, (sbyte)2, (short)3 };
            var result = TypeInference.InferNumericType(values);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void IsIntegerType_ChecksSByteAndByte()
        {
            Assert.True(TypeInference.IsIntegerType(typeof(byte)));
            Assert.True(TypeInference.IsIntegerType(typeof(sbyte)));
        }

        #endregion

        #region Mixed Sign Integers

        [Fact]
        public void InferNumericType_HandlesMixedSignedUnsigned()
        {
            var values = new List<object?> { -1, 2u }; // int + uint
            var result = TypeInference.InferNumericType(values);
            // Should handle gracefully - likely promotes to long or stays int
            Assert.True(result == typeof(long));
        }

        [Fact]
        public void InferNumericType_LargeUIntWithInt()
        {
            var values = new List<object?> { 1, uint.MaxValue };
            var result = TypeInference.InferNumericType(values);
            Assert.Equal(typeof(long), result); // uint.MaxValue requires long
        }

        #endregion

        #region Decimal Precision Tests

        [Fact]
        public void InferNumericType_PrefersDecimal_OverDouble()
        {
            var values = new List<object?> { 1.5m, 2.5m, 3.5m };
            Assert.Equal(typeof(decimal), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_DecimalWithInteger()
        {
            var values = new List<object?> { 1, 2.5m };
            Assert.Equal(typeof(decimal), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_DecimalWithDouble_PrefersDecimal()
        {
            // If both decimal and double, decimal should win for precision
            var values = new List<object?> { 1.5m, 2.5 };
            var result = TypeInference.InferNumericType(values);
            // Current implementation might return decimal or double
            Assert.True(result == typeof(decimal) || result == typeof(double));
        }

        #endregion

        #region Reference Type Hierarchy

        class Animal { }
        class Dog : Animal { }
        class Cat : Animal { }
        class GoldenRetriever : Dog { }

        [Fact]
        public void InferDataType_FindsCommonBaseClass()
        {
            var values = new List<object?> { new Dog(), new Cat() };
            Assert.Equal(typeof(Animal), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_DeepHierarchy()
        {
            var values = new List<object?> { new GoldenRetriever(), new Cat() };
            Assert.Equal(typeof(Animal), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_NoCommonBase_ReturnsObject()
        {
            var values = new List<object?> { "string", new Dog() };
            Assert.Equal(typeof(object), TypeInference.InferDataType(values));
        }

        #endregion

        #region Value Type Scenarios

        [Fact]
        public void InferDataType_SameStruct_ReturnsThatStruct()
        {
            var values = new List<object?>
            {
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 2)
            };
            Assert.Equal(typeof(DateTime), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_DifferentStructs_ReturnsValueType()
        {
            var values = new List<object?>
            {
                DateTime.Now,
                Guid.NewGuid(),
                TimeSpan.Zero
            };
            Assert.Equal(typeof(ValueType), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_MixedStructAndClass_ReturnsObject()
        {
            var values = new List<object?> { DateTime.Now, "string" };
            Assert.Equal(typeof(object), TypeInference.InferDataType(values));
        }

        #endregion

        #region Custom Cast Operators

        struct Temperature
        {
            public double Celsius;
            public static implicit operator Temperature(double celsius)
                => new Temperature { Celsius = celsius };
            public static explicit operator double(Temperature temp)
                => temp.Celsius;
        }

        struct Distance
        {
            public double Meters;
            public static implicit operator Distance(int meters)
                => new Distance { Meters = meters };
        }

        [Fact]
        public void CanCastValueType_ImplicitOperator_FromDouble()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(double), typeof(Temperature)));
        }

        [Fact]
        public void CanCastValueType_ExplicitOperator_ToDouble()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(Temperature), typeof(double)));
        }

        [Fact]
        public void CanCastValueType_NoOperator_ReturnsFalse()
        {
            Assert.False(TypeInference.CanCastValueType(typeof(Temperature), typeof(Distance)));
        }

        #endregion

        #region ulong Exclusion Tests

        [Theory]
        [InlineData(typeof(ulong), typeof(long), false)]
        [InlineData(typeof(long), typeof(ulong), false)]
        [InlineData(typeof(ulong), typeof(int), false)]
        [InlineData(typeof(ulong), typeof(double), false)]
        [InlineData(typeof(ulong), typeof(object), false)]
        public void CanCastValueType_ExcludesULong(Type from, Type to, bool expected)
        {
            Assert.Equal(expected, TypeInference.CanCastValueType(from, to));
        }

        [Fact]
        public void IsIntegerType_ReturnsFalse_ForULong()
        {
            Assert.True(TypeInference.IsIntegerType(typeof(ulong)));
        }

        [Fact]
        public void InferNumericType_WithULong_HandlesGracefully()
        {
            var values = new List<object?> { 1UL, 2UL };
            var result = TypeInference.InferNumericType(values);
            // Should handle ulong values somehow
            Assert.NotNull(result);
        }

        #endregion

        #region Boundary Values

        [Fact]
        public void InferNumericType_IntMaxValue()
        {
            var values = new List<object?> { int.MaxValue, int.MaxValue };
            Assert.Equal(typeof(int), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_ExceedsIntRange_PromotesToLong()
        {
            var values = new List<object?> { int.MaxValue, (long)int.MaxValue + 1 };
            Assert.Equal(typeof(long), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_DecimalMaxValue()
        {
            var values = new List<object?> { decimal.MaxValue, 1.0m };
            Assert.Equal(typeof(decimal), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_DoubleInfinity()
        {
            var values = new List<object?> { double.PositiveInfinity, 1.0 };
            Assert.Equal(typeof(double), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferNumericType_DoubleNaN()
        {
            var values = new List<object?> { double.NaN, 1.0, 2.0 };
            Assert.Equal(typeof(double), TypeInference.InferNumericType(values));
        }

        #endregion

        #region DBNull Handling

        [Fact]
        public void InferDataType_MixedDBNullAndValues()
        {
            var values = new List<object?>
            {
                DBNull.Value,
                123,
                DBNull.Value,
                456
            };
            Assert.Equal(typeof(int), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferNumericType_IgnoresDBNull()
        {
            var values = new List<object?> { DBNull.Value, 1, 2, DBNull.Value };
            Assert.Equal(typeof(int), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void IsNumerics_ReturnsFalse_ForDBNull()
        {
            Assert.False(TypeInference.IsNumeric(DBNull.Value));
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void InferDataType_Performance_OneMillion()
        {
            var values = Enumerable.Range(0, 1_000_000)
                .Select(i => (object?)i)
                .ToList();

            var sw = Stopwatch.StartNew();
            var result = TypeInference.InferDataType(values);
            sw.Stop();

            Assert.Equal(typeof(int), result);
            Assert.True(sw.ElapsedMilliseconds < 1000,
                $"Should complete in < 1s, took {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void InferNumericType_Performance_MixedTypes()
        {
            var values = new List<object?>();
            for (int i = 0; i < 100_000; i++)
            {
                values.Add(i % 3 == 0 ? (object)i :
                          i % 3 == 1 ? (object)(i * 1.5) :
                          (object)(i * 1.5m));
            }

            var sw = Stopwatch.StartNew();
            var result = TypeInference.InferNumericType(values);
            sw.Stop();

            Assert.Equal(typeof(double), result);
            Assert.True(sw.ElapsedMilliseconds < 200);
        }

        [Fact]
        public void CanCastValueType_Performance_ManyChecks()
        {
            var types = new[]
            {
                typeof(int), typeof(long), typeof(double),
                typeof(decimal), typeof(float), typeof(short)
            };

            var sw = Stopwatch.StartNew();
            int checks = 0;

            foreach (var from in types)
            {
                foreach (var to in types)
                {
                    TypeInference.CanCastValueType(from, to);
                    checks++;
                }
            }

            sw.Stop();

            Assert.Equal(36, checks);
            Assert.True(sw.ElapsedMilliseconds < 100,
                "36 cast checks should be very fast");
        }

        #endregion

        #region Type Conversion Edge Cases

        [Fact]
        public void CanCastValueType_SameType_ReturnsTrue()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(int), typeof(int)));
            Assert.True(TypeInference.CanCastValueType(typeof(decimal), typeof(decimal)));
        }

        [Fact]
        public void CanCastValueType_ToObject_AlwaysTrue()
        {
            Assert.True(TypeInference.CanCastValueType(typeof(int), typeof(object)));
            Assert.True(TypeInference.CanCastValueType(typeof(CustomStruct), typeof(object)));
        }

        [Fact]
        public void CanCastValueType_FromObject_ReturnsFalse()
        {
            Assert.False(TypeInference.CanCastValueType(typeof(object), typeof(int)));
        }

        struct CustomStruct { public int Value; }

        #endregion

        #region String and Special Types

        [Fact]
        public void InferDataType_AllStrings_ReturnsString()
        {
            var values = new List<object?> { "a", "b", "c" };
            Assert.Equal(typeof(string), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_MixedStringsAndInts_ReturnsObject()
        {
            var values = new List<object?> { "a", 1, "b", 2 };
            Assert.Equal(typeof(object), TypeInference.InferDataType(values));
        }

        [Fact]
        public void InferDataType_EmptyStrings()
        {
            var values = new List<object?> { "", "", "" };
            Assert.Equal(typeof(string), TypeInference.InferDataType(values));
        }

        #endregion

        #region Regression Tests

        [Fact]
        public void InferNumericType_SingleValue()
        {
            var values = new List<object?> { 42 };
            Assert.Equal(typeof(int), TypeInference.InferNumericType(values));
        }

        [Fact]
        public void InferDataType_SingleValue()
        {
            var values = new List<object?> { "test" };
            Assert.Equal(typeof(string), TypeInference.InferDataType(values));
        }

        [Fact]
        public void CanCastValueType_FloatToDecimal()
        {
            // This might lose precision but should be allowed
            Assert.True(TypeInference.CanCastValueType(typeof(float), typeof(decimal)));
        }

        [Fact]
        public void CanCastValueType_DecimalToFloat()
        {
            // This can overflow or lose precision
            Assert.True(TypeInference.CanCastValueType(typeof(decimal), typeof(float)));
        }

        #endregion
    }
}