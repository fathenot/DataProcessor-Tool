using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                return this.list.Contains(value);
            }

            public T GetItem(int index)
            {
                if (index < 0 || index >= list.Count)
                    throw new IndexOutOfRangeException("Index is out of range.");
                return list[index];
            }
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
    }
}
