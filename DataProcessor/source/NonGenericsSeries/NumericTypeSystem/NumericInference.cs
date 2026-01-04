using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries.NumericTypeSystem
{
    internal static class NumericInference
    {
        /// <summary>
        /// Determines the numeric kind of the specified value.
        /// </summary>
        /// <param name="v">The value to evaluate. Must not be <see langword="null"/> or <see cref="DBNull.Value"/>.</param>
        /// <returns>A <see cref="NumericKind"/> value representing the numeric type of the specified value.  Returns <see
        /// cref="NumericKind.Int32"/> for 32-bit integers, <see cref="NumericKind.Int64"/> for 64-bit integers,  <see
        /// cref="NumericKind.UInt32"/> for unsigned 32-bit integers, <see cref="NumericKind.UInt64"/> for unsigned
        /// 64-bit integers,  <see cref="NumericKind.Double"/> for floating-point numbers, <see
        /// cref="NumericKind.Decimal"/> for decimal numbers,  or <see cref="NumericKind.Object"/> if the value does not
        /// match any recognized numeric type.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="v"/> is <see langword="null"/> or <see cref="DBNull.Value"/>.</exception>
        internal static NumericKind GetNumericKind(object? v)
        {
            if (v is null || v == DBNull.Value)
                throw new InvalidOperationException("Null values should be skipped");

            return v switch
            {
                int or short or byte or sbyte or ushort => NumericKind.Int32,
                long => NumericKind.Int64,
                uint => NumericKind.UInt32,
                ulong => NumericKind.UInt64,
                float or double => NumericKind.Double,
                decimal => NumericKind.Decimal,
                _ => NumericKind.Object
            };
        }

        /// <summary>
        /// Infers the most appropriate numeric <see cref="Type"/> for a list of objects
        /// based on their runtime types and this library numeric promotion rules.
        /// </summary>
        /// <param name="values">The list of values to infer the type from. Nulls and <see cref="DBNull"/> are skipped.</param>
        /// <returns>
        /// The <see cref="Type"/> representing the inferred numeric type.
        /// <list type="bullet">
        /// <item>If all values are integers (signed/unsigned), the type is promoted to safely contain all values.</item>
        /// <item>If decimal values are present, the result is <see cref="decimal"/>.</item>
        /// <item>If double or float values are present, the result is <see cref="double"/>.</item>
        /// <item>If a non-numeric value is present, the result is <see cref="object"/>.</item>
        /// <item>If the list is empty or only contains null/DBNull, the result is <see cref="object"/>.</item>
        /// </list>
        /// </returns>
        internal static Type InferNumericType(IEnumerable<object?> values)
        {
            NumericKind? resultKind = null;

            foreach (var v in values)
            {
                if (v is null || v == DBNull.Value) continue;

                var kind = GetNumericKind(v);
                if (kind == NumericKind.Object)
                    return typeof(object);

                resultKind = resultKind is null ? kind : NumericPromotion.Promote(resultKind.Value, kind);
            }

            if (resultKind == null) return typeof(object);

            return resultKind switch
            {
                NumericKind.Int32 => typeof(int),
                NumericKind.Int64 => typeof(long),
                NumericKind.UInt32 => typeof(uint),
                NumericKind.UInt64 => typeof(ulong),
                NumericKind.Decimal => typeof(decimal),
                NumericKind.Double => typeof(double),
                _ => typeof(object)
            };
        }
    }
}
