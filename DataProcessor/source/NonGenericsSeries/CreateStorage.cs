using DataProcessor.source.ValueStorage;
namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        internal static AbstractValueStorage CreateValueStorage(List<object?> elements, bool copy = false)
        {
            var dataType = Support.InferDataType(elements);
            if (dataType == typeof(bool))
            {
                // If the data type is boolean, we can use BoolStorage
                if (elements.AsParallel().Any(x => x == null))
                    return new BoolStorage(elements.Select(e => e == null ? (bool?)null : Convert.ToBoolean(e)).ToArray());
                return new BoolStorage(elements.Select(Convert.ToBoolean).ToArray());

            }
            if (dataType == typeof(object))
            {
                return new ObjectValueStorage(elements.ToArray(), copy);
            }
            else if (dataType == typeof(string))
            {
                if (elements.AsParallel().Any(x => x == null))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (string?)null : Convert.ToString(e)).ToArray();
                    return new StringStorage(elementsWithNulls);
                }
                return new StringStorage(elements.Select(Convert.ToString).ToArray(), copy);
            }
            else if (dataType == typeof(double))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (double?)null : Convert.ToDouble(e)).ToArray();
                    return new DoubleValueStorage(elementsWithNulls);
                }
                return new DoubleValueStorage(elements.Select(Convert.ToDouble).ToArray(), false);
            }
            else if (dataType == typeof(decimal))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (decimal?)null : Convert.ToDecimal(e)).ToArray();
                    return new DecimalStorage(elementsWithNulls);
                }
                return new DecimalStorage(elements.Select(Convert.ToDecimal).ToArray(), false);
            }
            else if (dataType == typeof(DateTime))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (DateTime?)null : Convert.ToDateTime(e)).ToArray();
                    return new DateTimeStorage(elementsWithNulls);
                }
                return new DateTimeStorage(elements.Select(Convert.ToDateTime).ToArray());
            }
            else if (dataType == typeof(char))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (char?)null : Convert.ToChar(e)).ToArray();
                    return new CharStorage(elementsWithNulls);
                }
                return new CharStorage(elements.Select(Convert.ToChar).ToArray(), false);
            }
            else if(dataType == typeof(int))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (int?)null : Convert.ToInt32(e)).ToArray();

                    return new Int32ValuesStorage(elementsWithNulls);
                }
                return new Int32ValuesStorage(elements.Select(Convert.ToInt32).ToArray(), false);
            }
            else if (dataType == typeof(long))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (long?)null : Convert.ToInt64(e)).ToArray();

                    return new Int64ValuesStorage(elementsWithNulls);
                }
                return new Int64ValuesStorage(elements.Select(Convert.ToInt64).ToArray(), false);
            }
            return new ObjectValueStorage(elements.ToArray(), copy);

        }

        internal static object? ChangeType(object? value, Type toType)
        {
            if (value == null)
            {
                // Trả null nếu kiểu đích là nullable hoặc reference type
                if (!toType.IsValueType || Nullable.GetUnderlyingType(toType) != null)
                    return null;

                throw new InvalidCastException($"Cannot convert null to non-nullable type {toType.Name}");
            }

            Type valueType = value.GetType();

            // Nếu đã đúng kiểu thì return luôn
            if (toType.IsAssignableFrom(valueType))
                return value;

            // Nếu kiểu đích là nullable, lấy kiểu gốc ra
            Type underlyingType = Nullable.GetUnderlyingType(toType) ?? toType;

            // Handle Enum
            if (underlyingType.IsEnum)
            {
                if (value is string s)
                    return Enum.Parse(underlyingType, s, ignoreCase: true);
                else
                    return Enum.ToObject(underlyingType, value);
            }

            // Special handling for boolean
            if (underlyingType == typeof(bool))
            {
                if (value is string str)
                {
                    if (bool.TryParse(str, out bool boolResult))
                        return boolResult;
                    if (int.TryParse(str, out int intVal))
                        return intVal != 0;
                    throw new InvalidCastException($"Cannot convert '{str}' to bool.");
                }

                if (value is int i)
                    return i != 0;
            }

            // Handle Guid
            if (underlyingType == typeof(Guid))
            {
                if (value is string s)
                    return Guid.Parse(s);
                throw new InvalidCastException($"Cannot convert {valueType.Name} to Guid.");
            }

            // Nếu là kiểu chuyển đổi cơ bản
            try
            {
                return Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Cannot convert {value} (type {valueType.Name}) to {toType.Name}.", ex);
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="AbstractValueStorage"/> to store a collection of values  of the specified
        /// type, with optional copying of the input elements.
        /// </summary>
        /// <remarks>The method dynamically determines the appropriate storage type based on the provided
        /// <paramref name="type"/>  and the presence of null values in <paramref name="elements"/>. If the type is a
        /// value type and null values  are present, a nullable version of the type is used for storage. Specialized
        /// storage implementations are  used for common types such as <see cref="bool"/>, <see cref="string"/>, <see
        /// cref="double"/>, and others.</remarks>
        /// <param name="type">The type of the values to be stored. Must not be <see langword="null"/>.</param>
        /// <param name="elements">A list of values to be stored. Must not be <see langword="null"/>.  Null values in the list may result in the
        /// use of nullable types for storage.</param>
        /// <param name="copy">A <see langword="bool"/> indicating whether the input elements should be copied  into the storage. If <see
        /// langword="false"/>, the storage may directly reference the input elements.</param>
        /// <returns>An instance of <see cref="AbstractValueStorage"/> capable of storing the provided values  with the specified
        /// type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> or <paramref name="elements"/> is <see langword="null"/>.</exception>
        internal static AbstractValueStorage CreateValueStorage(Type type, List<object?> elements, bool copy)
        {
            //check valid arguments
            if (type == null) throw new ArgumentNullException($"data type must not be null {nameof(type)}");
            if (elements == null) throw new ArgumentNullException($"{nameof(elements)} must not be null");

            // fault tolerance if type is value type and not nullable but elements contains null -> change type
            Type? changedType = null;
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                if (elements.AsParallel().Any(e => e == null))
                {
                    changedType = typeof(Nullable<>).MakeGenericType(type);
                }
            }

            //change data type of all elements
            object?[] changedTypeElements;
            if (changedType == null)
            {
                changedTypeElements = elements.Select(element => ChangeType(element, type)).ToArray();
            }
            else
            {
                changedTypeElements = elements.Select(element => ChangeType(element, changedType)).ToArray();
            }


            // create storage
            if (type == typeof(bool))
            {
                // If the data type is boolean, we can use BoolStorage
                if (elements.AsParallel().Any(x => x == null))
                    return new BoolStorage(elements.Select(e => e == null ? (bool?)null : Convert.ToBoolean(e)).ToArray());
                return new BoolStorage(elements.Select(Convert.ToBoolean).ToArray());
            }
            if (type == typeof(object))
            {
                return new ObjectValueStorage(elements.ToArray(), copy);
            }
            else if (type == typeof(string))
            {
                if (elements.AsParallel().Any(element => element == null))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (string?)null : Convert.ToString(e)).ToArray();
                    return new StringStorage(elementsWithNulls);
                }
                return new StringStorage(elements.Select(Convert.ToString).ToArray(), copy);
            }
            else if (type == typeof(double))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (double?)null : Convert.ToDouble(e)).ToArray();
                    return new DoubleValueStorage(elementsWithNulls);
                }
                return new DoubleValueStorage(elements.Select(Convert.ToDouble).ToArray(), false);
            }
            else if (type == typeof(decimal))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (decimal?)null : Convert.ToDecimal(e)).ToArray();
                    return new DecimalStorage(elementsWithNulls);
                }
                return new DecimalStorage(elements.Select(Convert.ToDecimal).ToArray(), false);
            }
            else if (type == typeof(DateTime))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (DateTime?)null : Convert.ToDateTime(e)).ToArray();
                    return new DateTimeStorage(elementsWithNulls);
                }
                return new DateTimeStorage(elements.Select(Convert.ToDateTime).ToArray());
            }
            else if (type == typeof(char))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (char?)null : Convert.ToChar(e)).ToArray();
                    return new CharStorage(elementsWithNulls);
                }
                return new CharStorage(elements.Select(Convert.ToChar).ToArray(), false);
            }
            else if (Support.IsIntegerType(type))
            {
                if ((elements.AsParallel().Any(element => element == null)))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (long?)null : Convert.ToInt64(e)).ToArray();
                    return new Int64ValuesStorage(elementsWithNulls);
                }
                return new Int64ValuesStorage(elements.Select(Convert.ToInt64).ToArray(), false);
            }
            return new ObjectValueStorage(elements.ToArray(), copy);

        }
    }
}
