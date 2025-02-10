using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public class Series : ICollection<object>, IEnumerable<object>
    {
        private string name;
        private IList<object> values;
        
        public Series(string name, IList<object> values)
        {
            this.name = name;
            this.values = values;
        }

        public Series(Series other)
        {
            this.name = other.name;
            this.values = new List<object>(other.values);
        }

        public Series(Tuple<string, IList<object>> KeyAndValues)
        {
            this.name = KeyAndValues.Item1;
            this.values = new List<object> (KeyAndValues.Item2);
        }

        // utility
        public object Name { get { return this.name; } }
        public IList<object> Values { get { return this.values; } }
        public int Count => values.Count;
        public bool IsReadOnly { get { return false; } }
        public object this[int index] => values[index];

        // iterator
        public IEnumerator<object> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        //method
        public void Add(object item)
        {
            values.Add(item);
        }
        public bool Remove(object item)
        {
            return values.Remove(item);
        }
        public void Clear()
        {
            values.Clear();
        }
        public bool Contains(object item)
        {
            return values.Contains(item);
        }
        public void CopyTo(object[] array, int arrayIndex)
        {
            values.CopyTo((object[])array, arrayIndex);
        }
        
        public IList<object> getItem(int indexFrom, int indexTo, int step = 1)
        {
            IList<object> result = new List<object>();

            // check valid argument
            if(indexFrom < 0 || indexTo < 0 || indexFrom >= values.Count || indexTo >= values.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }

            if (step < 0)
            {
                if (indexFrom < indexTo)
                {
                    throw new ArgumentException("Index from must be greater than index to when step < 0");
                }
            }

            if(step > 0 && indexFrom > indexTo)
            {
                throw new ArgumentException("Index from must be smaller than index to when step > 0");
            }

            if (step > values.Count)
            {
                throw new ArgumentOutOfRangeException("Step greater than size of series");
            }
            
            if(step == 0)
            {
                throw new ArgumentException("Step must not be zero");
            }
            
            //main logic of the method
            if (indexTo == indexFrom)
            {
                return result;
            }
            for (int i = indexFrom; i != indexTo; i += step)
            {
                result.Add(values[i]);
            }
            return result;
        }

        public void Sort(Comparer<object>? comparer = null)
        {
            if (comparer == null)
            {
                // Sử dụng OrderBy với logic null được đưa về cuối
                values = values.OrderBy(x => x == null ? 1 : 0)
                               .ThenBy(x => x)  // Đảm bảo sắp xếp bình thường sau khi xử lý null
                               .ToList();
            }
            else
            {
                // Sử dụng OrderBy với comparer, với logic null được đưa về cuối
                values = values.OrderBy(x => x == null ? 1 : 0)
                               .ThenBy(x => x, comparer)  // Sắp xếp với comparer nếu có
                               .ToList();
            }
        }

        public void changeItem(int index, object? item)
        {
            if (index < 0 || index >= values.Count)
            {
                throw new ArgumentOutOfRangeException("index is out of range");
            }
            this.values[index] = item;
        }
        // searching and filter
        public IList<object> Filter(Func<object, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }
        public IList<int> Find(object? item)
        {
            IList<int> Indexes = new List<int>();
            for(int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }
        public static void print(Series series)
        {
            Console.WriteLine(series.Name);
            int rowIndex = 1;
            foreach (var item in series.Values)
            {
                Console.WriteLine($"{rowIndex} {item}");
            }
        }
    }
}
