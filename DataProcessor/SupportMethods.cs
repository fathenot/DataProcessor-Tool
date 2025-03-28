using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    // this is class contains methods only used by this library developers and the name of class may be changed in the future
    static internal class Supporter
    {
        public class OrderedSet<T> : IEnumerable<T>
        {
            private readonly HashSet<T> set = new HashSet<T>();
            private readonly List<T> list = new List<T>();

            public bool Add(T item)
            {
                if (set.Add(item))
                {
                    list.Add(item);
                    return true;
                }
                return false;
            }

            public bool Remove(T item)
            {
                if (set.Remove(item))
                {
                    list.Remove(item);
                    return true;
                }
                return false;
            }

            public void Clear()
            {
                set.Clear();
                list.Clear();
            }

            public bool Contains(T value)
            {
                return this.set.Contains(value);
            }

            public T GetItem(int index)
            {
                if (index < 0 || index >= list.Count)
                    throw new IndexOutOfRangeException("Index is out of range.");
                return list[index];
            }

            public List<T> getData() => list;
            public int Count => list.Count;

            public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        internal static void CheckNull(object? value)
        {
            if(value == null)
            {
                throw new Exception($"{nameof(value)} is null");
            }
        }

        internal static bool IsNumerics(object? value)
        {
            return value is sbyte || value is byte || value is short
                  || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
        }

        internal static Type InferNumericType(List<object?> values)
        {
            bool hasDecimal = false;
            bool hasDouble = false;
            bool hasInt = false;

            foreach (var v in values)
            {
                if (v == null || v == DBNull.Value) continue;

                switch (v)
                {
                    case int or long or short or byte:
                        hasInt = true;
                        break;
                    case float or double:
                        hasDouble = true;
                        break;
                    case decimal:
                        hasDecimal = true;
                        break;
                    default:
                        return typeof(object); // Không phải số → object
                }
            }
            if (hasDecimal && values.Where(v => v is IConvertible && v != null && v != DBNull.Value)
                        .Any(v =>
                        {
                            try
                            {
                                return Convert.ToDecimal(v) > decimal.MaxValue;
                            }
                            catch
                            {
                                return false;
                            }
                        }))
            {
                return typeof(double);
            }
            return hasDecimal ? typeof(decimal)
                 : hasDouble ? typeof(double)
                 : hasInt ? typeof(long)
                 : typeof(object); // Trường hợp danh sách rỗng hoặc chỉ chứa null
        }

        internal static Type InferDataType(List<object?> values)
        {
            var nonNullValues = values.Where(v => v != null && v != DBNull.Value).ToList();
            if (nonNullValues.Count == 0)
            {
                return typeof(object); // Trả về object nếu chỉ chứa null/DBNull
            }

            bool AllNumerics = values.All(v => v != null && v != DBNull.Value && IsNumerics(v));
            if (AllNumerics)
            {
                return InferNumericType(values);
            }
            // check values contains value type or reference type
            bool ContainsValueType = values.Any(v => (v!= null && v!= DBNull.Value) && v.GetType().IsValueType);
            bool ContainsReferenceType = values.Any(v => (v != null && v != DBNull.Value) && !v.GetType().IsValueType);
            
            if(ContainsReferenceType && ContainsValueType)
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
                foreach(var obj in nonNullValues)
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
    }
}
