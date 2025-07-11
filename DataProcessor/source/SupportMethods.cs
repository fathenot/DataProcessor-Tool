using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source
{
    // this is class contains methods and class can only used by this library developers 
    // the name of class may be changed in the future
    static internal class Supporter
    {
        internal class OrderedSet<T> : IEnumerable<T>
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
                return set.Contains(value);
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

        internal static void ScaleLength(List<List<object?>> rows)
        {
            int maxRowLength = rows.Max(row => row.Count);

            foreach (var row in rows)
            {
                row.AddRange(Enumerable.Repeat<object?>(null, maxRowLength - row.Count));
            }
        }

        public static bool IsGroupedIndex(IEnumerable<object?> index)
        {
            if (index == null)
            {
                return false;
            }
            foreach (var item in index)
            {
                if (item is object[] || item is List<object> ||
                    item?.GetType().FullName?.StartsWith("System.ValueTuple") == true ||
                    item is ITuple)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
