using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataProcessor
{
    public interface ISeries
    {
        string? Name { get; }
        IReadOnlyList<object> Values { get; }
        Type dType { get;}
        bool IsReadOnly { get; }
        void Clear();
        void Add(object? item);
        public bool IsValidType(object? value);
        public int Count { get; }
    }
    public class Series : ISeries, ICollection<object>, IEnumerable
    {
        private string? name;
        private IList<object>? values;
        private Type dtype;

        // support method to check type is valid to add the data frame
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : value.GetType() == dType;
        }

        //constructor
        public Series(string? name, IList<object>? values)
        {
            if(name == null && values == null)
            {
                throw new ArgumentNullException("At least name or values not null");
            }
            this.name = name;
            this.values = values;
            if(values != null)
            {
                var nonNullTypes = values.Where(v => v != null).Select(v => v.GetType()).Distinct();
                dType = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            }
            else
            {
                this.dType = typeof(object);
            }
            this.dtype = dType;
        }
        
        public Series(Series other)
        {
            if(other == null)
            {
                throw new ArgumentNullException("other series must not be null");
            }
            ArgumentNullException.ThrowIfNull(other);
            this.name = other.name;
            if (other.values != null)
            {
                this.values = new List<object>(other.values);
            }
            else if (other.values == null)
            {
                this.values = null;
            }
            this.dtype = other.dType;
        }

        public Series(Tuple<string, IList<object>> KeyAndValues)
        {
            if(KeyAndValues == null)
            {
                throw new ArgumentNullException($"KeyAndValues {nameof(KeyAndValues)} must not be null");
            }
            this.name = KeyAndValues.Item1;
            this.values = new List<object>(KeyAndValues.Item2);
            var tempValuesList = KeyAndValues.Item2;
            var nonNullTypes = tempValuesList.Where(v => v != null).Select(v => v.GetType()).Distinct();
            dType = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            this.dtype = dType;
        }

        // Properties
        public string? Name { get { return this.name; } }
        public IReadOnlyList<object> Values => values == null ? throw new Exception("values list is null") : values as IReadOnlyList<object> ?? values.ToList();
        public int Count => values == null ? 0 : values.Count;
        public bool IsReadOnly { get { return false; } }
        public object this[int index]
        {
            get
            {
                if(values == null)
                {
                    throw new Exception("values list is null");
                }
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                return values[index];
            }
            set
            {
                if (values == null)
                {
                    throw new Exception("values list is null");
                }
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                if (!IsValidType(value))
                    throw new ArgumentException($"Expected type {dType}, but got {value?.GetType()}");
                values[index] = value;
            }
        }
        public Type dType
        {
            get => dtype;
            private set => dtype = value;
        }

        // iterator
        public IEnumerator<object> GetEnumerator()
        {
            if(values == null)
            {
                throw new Exception("values list is null");
            }
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (values == null)
            {
                throw new Exception("values list is null");
            }
            return values.GetEnumerator();
        }

        //method
        public void Add(object? item)
        {
            if (!IsValidType(item))
            {
                throw new ArgumentException($"Expected type {dType}, but got {item?.GetType()}");
            }
            if(values == null)
            {
                this.values = new List<object>();
                values.Add(item);
            }
        }
        public bool Remove(object item)
        {
            return values == null? false : values.Remove(item);
        }
        public void Clear()
        {
            if (values != null) values.Clear();
        }
        public bool Contains(object item)
        {
            return values == null ? false : values.Contains(item);
        }
        public void CopyTo(object[] array, int arrayIndex)
        {
            if(values == null)
            {
                return;
            }
            values.CopyTo((object[])array, arrayIndex);
        }
        
        // truy xuất
        public IList<object> GetItem(int indexFrom, int indexTo, int step = 1)
        {
            IList<object> result = new List<object>();

            // check valid argument
            if (indexFrom < 0 || indexTo < 0 || values == null || indexFrom >= values.Count || indexTo >= values.Count)
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
                return result;  // Trả về danh sách rỗng theo phong cách Python
            }
            for (int i = indexFrom; i != indexTo; i += step)
            {
                result.Add(values[i]);
            }
            return result;
        }

        // change type of data
        public Series Astype(Type newType)
        {
            if (newType == null) throw new ArgumentNullException(nameof(newType));

            if(this.values == null)
            {
                return new Series(this.Name, null);
            }
            IList<object> newValues = values.Select(v =>
            {
                if (v == null || v == DBNull.Value) return DBNull.Value;

                try
                {
                    if (newType == typeof(DateTime) && v is string str)
                    {
                        return DateTime.TryParse(str, out DateTime dt) ? dt : DBNull.Value;
                    }
                    return Convert.ChangeType(v, newType);
                }
                catch
                {
                    return DBNull.Value; // Nếu lỗi thì giữ nguyên giá trị null
                }
            }).ToList();

            return new Series(this.Name, newValues);
        }

        public void Sort(Comparer<object>? comparer = null)
        {
            if(values == null)
            {
                return;
            }
            if (comparer == null)
            {
                if (values.Any(x => x != null && x is not IComparable))
                {
                    throw new InvalidOperationException("All non-null values must implement IComparable for sorting.");
                }
                // Sử dụng OrderBy với logic null được đưa về cuối
                values = values.OrderBy(x => x != null)
                               .ThenBy(x => x as IComparable)  // Đảm bảo sắp xếp bình thường sau khi xử lý null
                               .Concat(values.Where(x => x == null))
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
            if(values == null)
            {
                throw new ArgumentException($"list of value is null");
            }
            if (!IsValidType(item))
            {
                throw new ArgumentException($"Expected type {dType}, but got {item?.GetType()}");
            }
            if (index < 0 || index >= values.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
            }
            values[index] = item ?? DBNull.Value; // Dùng DBNull.Value để thể hiện null trong Series
        }

        // searching and filter
        public IList<object> Filter(Func<object, bool> filter)
        {
            if(values == null)
            {
                throw new Exception($"Values list is null");
            }
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
        // show the series
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


    public class Series<DataType> : ICollection<DataType>, ISeries
    {
        private string name;
        private IList<DataType> values;

        public Series(string name, IList<DataType> values)
        {
            this.name = name;
            this.values = values;
        }

        public Series(Series <DataType> other)
        {
            this.name = other.name;
            this.values = new List<DataType>((IEnumerable<DataType>)other.Values);
        }

        public Series(Tuple<string, IList<DataType>> KeyAndValues)
        {
            this.name = KeyAndValues.Item1;
            this.values = new List<DataType>(KeyAndValues.Item2);
        }

        // utility
        public String Name { get { return this.name; } }
        public int Count => values.Count;
        public bool IsReadOnly => false;
        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                if(values == null)
                {
                    throw new ArgumentException("list of values is null");
                }
                return values[index];
            }
            set
            {
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                values[index] = (DataType)value;
            }
        }
        public Type dType => typeof(DataType);
        public IReadOnlyList<object> Values => values.Cast<object>().ToList();


        // iterator
        public IEnumerator<DataType> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        //method 
        public void Add (object? item)
        {
            if (this.IsValidType(item)) values.Add((DataType?)item);

        }
        public void Add(DataType item)
        {
            values.Add(item);
        }
        public bool Remove(DataType item)
        {
            return values.Remove(item);
        }
        public void Clear()
        {
            values.Clear();
        }
        public bool Contains(DataType item)
        {
            return values.Contains(item);
        }
        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.CopyTo((DataType[])array, arrayIndex);
        }

        public IList<DataType> getItem(int indexFrom, int indexTo, int step = 1)
        {
            IList<DataType> result = new List<DataType>();

            // check valid argument
            if (indexFrom < 0 || indexTo < 0 || indexFrom >= values.Count || indexTo >= values.Count)
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

            if (step > 0 && indexFrom > indexTo)
            {
                throw new ArgumentException("Index from must be smaller than index to when step > 0");
            }

            if (step > values.Count)
            {
                throw new ArgumentOutOfRangeException("Step greater than size of series");
            }

            if (step == 0)
            {
                throw new ArgumentException("Step must not be zero");
            }

            //main logic of the method
            if (indexTo == indexFrom)
            {
                return result;  // Trả về danh sách rỗng theo phong cách Python
            }
            for (int i = indexFrom; i != indexTo; i += step)
            {
                result.Add(values[i]);
            }
            return result;
        }

        public void Sort(Comparer<DataType>? comparer = null)
        {
            if (comparer == null)
            {
                // Sử dụng OrderBy với logic null được đưa về cuối
                values = values.OrderBy(x => x != null)
                               .ThenBy(x => x as IComparable)  // Đảm bảo sắp xếp bình thường sau khi xử lý null
                               .Concat(values.Where(x => x == null))
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

        public void changeItem(int index, DataType item)
        {
            if (index < 0 || index >= values.Count)
            {
                throw new ArgumentOutOfRangeException("index is out of range");
            }
            this.values[index] = item;
        }
        // searching and filter
        public IList<DataType> Filter(Func<DataType, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }
        public IList<int> Find(DataType? item)
        {
            IList<int> Indexes = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }

        // print the series
        public static void print(Series<DataType> series)
        {
            Console.WriteLine(series.Name);
            int rowIndex = 1;
            foreach (var item in series.Values)
            {
                Console.WriteLine($"{rowIndex} {item}");
            }
        }
      
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : value.GetType() == dType;
        }
    }
  
}
