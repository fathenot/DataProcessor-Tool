using System.Reflection;
using DataProcessor.source.NonGenericsSeries.NumericTypeSystem;
namespace DataProcessor.source.NonGenericsSeries
{
    internal static class TypeInference
    {
        /// <summary>
        /// Determines whether the specified <see cref="Type"/> represents an integer numeric type
        /// according to the this library numeric type system.
        /// </summary>
        /// <remarks>
        /// This method classifies integer types by mapping the provided <see cref="Type"/> to its
        /// corresponding <see cref="NumericKind"/> and evaluating it using the numeric contract.
        /// <para>
        /// Supported integer types include signed and unsigned integral primitives such as
        /// <see cref="byte"/>, <see cref="sbyte"/>, <see cref="short"/>, <see cref="ushort"/>,
        /// <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>,
        /// <see cref="nint"/>, and <see cref="nuint"/>.
        /// </para>
        /// <para>
        /// This method reflects the logical numeric classification and is independent of
        /// storage or indexing constraints.
        /// </para>
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to evaluate. This parameter must not be <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Type"/> is classified as an integer numeric type;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        internal static bool IsIntegerType(Type type)
        {
            return NumericKindExtensions.IsInteger(
                NumericKindExtensions.GetNumericKind(type));
        }


        /// <summary>
        /// Determines whether the specified <see cref="Type"/> represents a floating-point numeric type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns><see langword="true"/> if the specified <see cref="Type"/> is <see cref="float"/> or <see cref="double"/>;
        /// otherwise, <see langword="false"/>.</returns>
        internal static bool IsFloatingType(Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> represents a numeric type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the specified <see cref="Type"/> represents a numeric type;  otherwise, <see
        /// langword="false"/>.</returns>
        internal static bool IsNumericType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) ||
           type == typeof(short) || type == typeof(ushort) ||
           type == typeof(int) || type == typeof(uint) ||
           type == typeof(long) || type == typeof(float) ||
           type == typeof(double) || type == typeof(decimal);
        }

        /// <summary>
        /// Determines whether the specified object represents a numeric value.
        /// </summary>
        /// <remarks>This method checks for common numeric types, including signed and unsigned integers,
        /// floating-point numbers, and decimals. Null values are not considered numeric.</remarks>
        /// <param name="value">The object to evaluate. Can be null.</param>
        /// <returns><see langword="true"/> if the object is a numeric type, such as <see cref="int"/>, <see cref="double"/>, or
        /// <see cref="decimal"/>; otherwise, <see langword="false"/>.</returns>
        internal static bool IsNumeric(object? value)
        {
            return value is sbyte || value is byte || value is short
                  || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
        }

        /// <summary>
        /// Infers the most appropriate numeric type based on the values in the provided list.
        /// </summary>
        /// <remarks>If the list contains a mix of numeric types, the method prioritizes the most precise
        /// type. For example, if both decimal and double values are present, <see cref="System.Decimal"/> is returned
        /// unless a value exceeds the range of <see cref="System.Decimal"/>, in which case <see cref="System.Double"/>
        /// is returned. Non-numeric values in the list are ignored during type inference.</remarks>
        /// <param name="values">A list of objects to analyze. The list may contain numeric values, nulls, or non-numeric values.</param>
        /// <returns>The inferred numeric type based on the values in the list: <list type="bullet"> <item><description><see
        /// cref="System.Decimal"/> if the list contains decimal values.</description></item> <item><description><see
        /// cref="System.Double"/> if the list contains double or float values.</description></item>
        /// <item><description><see cref="System.Int64"/> if the list contains integer values (e.g., int, long, short,
        /// byte).</description></item> <item><description><see cref="System.Object"/> if the list contains non-numeric
        /// values or is empty.</description></item> </list></returns>
        internal static Type InferNumericType(IEnumerable<object?> values)
        {
            return NumericTypeSystem.NumericInference.InferNumericType(values);
        }


        /// <summary>
        /// Infers the most appropriate data type for a collection of values.
        /// </summary>
        /// <remarks>This method analyzes the provided list to determine the most specific type that can
        /// represent its non-null values. If the list contains a mix of value types and reference types, <see
        /// cref="object"/> is returned. For numeric values, the method attempts to infer the most specific numeric
        /// type.</remarks>
        /// <param name="values">A list of values to analyze. The list may contain null or <see cref="DBNull.Value"/> entries.</param>
        /// <returns>The inferred <see cref="Type"/> based on the contents of the <paramref name="values"/> list: <list
        /// type="bullet"> <item><description><see cref="object"/> if the list contains only null or <see
        /// cref="DBNull.Value"/> entries.</description></item> <item><description>A numeric type (e.g., <see
        /// cref="int"/>, <see cref="double"/>) if all non-null values are numeric.</description></item>
        /// <item><description><see cref="ValueType"/> if the list contains multiple distinct value
        /// types.</description></item> <item><description>The common base type of all reference types if the list
        /// contains only reference types.</description></item> <item><description><see cref="object"/> if the list
        /// contains a mix of value types and reference types.</description></item> </list></returns>
        internal static Type InferDataType(List<object?> values)
        {
            var nonNullValues = values.Where(v => v != null && v != DBNull.Value).ToList();
            if (nonNullValues.Count == 0)
            {
                return typeof(object); // Trả về object nếu chỉ chứa null/DBNull
            }

            bool AllNumerics = values
                .Where(v => v != null && v != DBNull.Value) // Loại bỏ các giá trị null và DBNull.Value
                .All(v => IsNumeric(v)); // Kiểm tra tính số học của phần tử còn lại
            if (AllNumerics)
            {
                return InferNumericType(values);
            }

            // check values contains value type or reference type
            bool ContainsValueType = values.Any(v => v != null && v != DBNull.Value && v.GetType().IsValueType);
            bool ContainsReferenceType = values.Any(v => v != null && v != DBNull.Value && !v.GetType().IsValueType);

            if (ContainsReferenceType && ContainsValueType)
            {
                return typeof(object);
            }
            else if (ContainsValueType)// only contains values type
            {
                Type firstType = nonNullValues.First()?.GetType()!;
                if (nonNullValues.All(v => v!.GetType() == firstType))
                    return firstType;

                // Nếu có nhiều kiểu struct khác nhau → trả về ValueType
                return typeof(ValueType);
            }
            else if (ContainsReferenceType)
            {
                Type baseType = nonNullValues.First()!.GetType();
                foreach (var obj in nonNullValues)
                {
                    Type CurrentType = obj!.GetType();
                    while (!CurrentType.IsAssignableTo(baseType))
                    {
                        baseType = baseType.BaseType ?? typeof(object);
                    }
                }
                return baseType;
            }
            return typeof(object);
        }

        /// <summary>
        /// Determines whether a value of the specified source type can be cast to the specified target type.
        /// </summary>
        /// <remarks>This method evaluates type compatibility for value types, including handling nullable
        /// types, enums, numeric types, and the presence of custom implicit or explicit cast operators. It does not
        /// support casting to or from <see cref="ulong"/>.</remarks>
        /// <param name="fromType">The source type to cast from.</param>
        /// <param name="toType">The target type to cast to.</param>
        /// <returns><see langword="true"/> if a value of <paramref name="fromType"/> can be cast to <paramref name="toType"/>;
        /// otherwise, <see langword="false"/>.</returns>
        internal static bool CanCastValueType(Type fromType, Type toType)
        {
            // Không xét ulong
            if (fromType == typeof(ulong) || toType == typeof(ulong))
                return false;

            // Unwrap nullable
            fromType = Nullable.GetUnderlyingType(fromType) ?? fromType;
            toType = Nullable.GetUnderlyingType(toType) ?? toType;

            // Same type
            if (fromType == toType)
                return true;

            // Enum → underlying
            if (fromType.IsEnum)
                fromType = Enum.GetUnderlyingType(fromType);
            if (toType.IsEnum)
                toType = Enum.GetUnderlyingType(toType);

            // Numeric types
            Type[] numericTypes = new[]
            {
                typeof(byte), typeof(short), typeof(ushort),
                typeof(int), typeof(uint), typeof(long),
                typeof(float), typeof(double), typeof(decimal)
            };

            if (numericTypes.Contains(fromType) && numericTypes.Contains(toType))
            {
                try
                {
                    object dummy = Convert.ChangeType(0, fromType);
                    Convert.ChangeType(dummy, toType);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // Nếu to là object thì cast nào cũng được
            if (toType == typeof(object))
                return true;

            // Kiểm tra presence của implicit hoặc explicit operator
            return HasCustomCastOperator(fromType, toType);
        }

        /// <summary>
        /// Determines whether a custom cast operator exists between two types.
        /// </summary>
        /// <remarks>This method checks both the source type (<paramref name="fromType"/>) and the target
        /// type  (<paramref name="toType"/>) for public static methods named "op_Implicit" or "op_Explicit"  that
        /// define a valid cast operator.</remarks>
        /// <param name="fromType">The source type to check for a cast operator.</param>
        /// <param name="toType">The target type to check for a cast operator.</param>
        /// <returns><see langword="true"/> if a custom implicit or explicit cast operator exists between  <paramref
        /// name="fromType"/> and <paramref name="toType"/>; otherwise, <see langword="false"/>.</returns>
        private static bool HasCustomCastOperator(Type fromType, Type toType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            return fromType.GetMethods(flags).Any(m =>
                (m.Name == "op_Implicit" || m.Name == "op_Explicit") &&
                m.ReturnType == toType &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == fromType)
            ||
            toType.GetMethods(flags).Any(m =>
                (m.Name == "op_Implicit" || m.Name == "op_Explicit") &&
                m.ReturnType == toType &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == fromType);
        }

    }
}
