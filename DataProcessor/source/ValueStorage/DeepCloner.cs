using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Provides functionality for creating deep copies of objects, including nested objects and collections.
/// </summary>
/// <remarks>This class supports cloning of complex objects, arrays, generic collections, and objects implementing
/// <see cref="ICloneable"/>. Immutable types, primitive types, and certain special cases (e.g., <see cref="string"/>, 
/// <see cref="decimal"/>, <see cref="DateTime"/>) are returned as-is without cloning. Anonymous types are not cloned; 
/// the original object is returned instead. The cloning process attempts to recursively copy readable and writable 
/// properties and fields for class types.</remarks>
internal static class UniversalDeepCloner
{
    /// <summary>
    /// Determines whether the specified <see cref="Type"/> represents an anonymous type.
    /// </summary>
    /// <remarks>Anonymous types are compiler-generated types that are typically used to encapsulate a set of
    /// properties. This method checks for characteristics specific to anonymous types, such as the presence of the 
    /// <see cref="System.Runtime.CompilerServices.CompilerGeneratedAttribute"/>, specific naming conventions,  and
    /// non-public accessibility.</remarks>
    /// <param name="type">The <see cref="Type"/> to evaluate.</param>
    /// <returns><see langword="true"/> if the specified <see cref="Type"/> is an anonymous type; otherwise, <see
    /// langword="false"/>.</returns>
    private static bool IsAnonymousType(Type type)
    {
        return Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
               && type.IsGenericType
               && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
    }

    /// <summary>
    /// Creates a deep copy of the specified object, including its nested objects and collections.
    /// </summary>
    /// <remarks>This method supports cloning of complex objects, including arrays, generic collections, and
    /// objects implementing <see cref="ICloneable"/>. For anonymous types, the original object is returned without
    /// cloning. If the object is a class with readable and writable properties or fields, the method attempts to create
    /// a deep copy by recursively cloning its members.</remarks>
    /// <param name="source">The object to be cloned. Can be <see langword="null"/>.</param>
    /// <returns>A deep copy of the <paramref name="source"/> object, or <see langword="null"/> if <paramref name="source"/> is
    /// <see langword="null"/>. Immutable types, primitive types, and certain special cases (e.g., <see cref="string"/>,
    /// <see cref="decimal"/>, <see cref="DateTime"/>) are returned as-is.</returns>
    internal static object? DeepClone(object? source)
    {
        //handle anonymous types
        if (IsAnonymousType(source?.GetType() ?? typeof(object)))
        {
            return source;
        }

        //check for null
        if (source == null)
            return null;

        Type type = source.GetType();

        // Trường hợp immutable hoặc đơn giản
        if (type.IsPrimitive || source is string || source is decimal || source is DateTime)
            return source;

        // Nếu là mảng
        if (type.IsArray)
        {
            Array srcArray = (Array)source;
            Array dstArray = (Array)Activator.CreateInstance(type, srcArray.Length)!;

            for (int i = 0; i < srcArray.Length; i++)
                dstArray.SetValue(DeepClone(srcArray.GetValue(i)), i);

            return dstArray;
        }

        // Nếu là danh sách
        if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
        {
            var listType = typeof(List<>).MakeGenericType(type.GetGenericArguments());
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in (IEnumerable)source)
                list.Add(DeepClone(item));

            return list;
        }

        // Nếu implement ICloneable
        if (source is ICloneable cloneable)
            return cloneable.Clone();

        // Nếu là class thường, cố gắng tạo bằng constructor mặc định
        object clone = Activator.CreateInstance(type)!;
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var value = prop.GetValue(source);
            prop.SetValue(clone, DeepClone(value));
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var value = field.GetValue(source);
            field.SetValue(clone, DeepClone(value));
        }

        return clone;
    }
}
