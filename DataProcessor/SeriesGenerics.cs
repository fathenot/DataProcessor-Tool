using DataProcessor;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public class SeriesSliceView<DataType> : ISeries<DataType> where DataType:notnull
    {
        private ISeries<DataType> original;
        private List<int> indicies;
        public SeriesSliceView(ISeries<DataType> original, ValueTuple<int, int, int> slice)
        {
            SupportMethods.CheckNull(original);
            if (slice.Item3 == 0)
            {
                throw new ArgumentException("step must not be zero");
            }
            if (slice.Item1 < 0 || slice.Item1 >= original.Count)
            {
                throw new ArgumentException("Start is out of range", nameof(slice.Item1));
            }
            if (slice.Item2 < 0 || slice.Item2 >= original.Count)
            {
                throw new ArgumentException("Start is out of range", nameof(slice.Item2));
            }

            this.original = original;
            indicies = new List<int>();

            if (slice.Item3 > 0)
            {
                for (int i = slice.Item1; i <= slice.Item2; i += slice.Item3)
                {
                    indicies.Add(i);
                }
            }
            else
            {
                for (int i = slice.Item1; i >= slice.Item2; i += slice.Item3)
                {
                    indicies.Add(i);
                }
            }
        }

        // utilities
        public int Count => indicies.Count;
        public Type dType => typeof(DataType);
        public bool IsReadOnly => false;
        public DataType this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return original[indicies[index]];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                original[indicies[index]] = value;
            }
        }
        public string? Name => original.Name;
        public IReadOnlyList<DataType> Values
        {
            get
            {
                var values = indicies.Select(index => original[index]).ToList();
                return values.AsReadOnly();
            }
        }
        public void Add(DataType item)
        {
            throw new NotSupportedException($"cannot add {nameof(item)} to the view of the series");
        }
        public bool Remove(DataType item) => throw new NotSupportedException($"cannot remove {nameof(item)} to the view of the series");
        public ISeries<DataType> AsType(Type NewType, bool ForceCast = false)
        {
            throw new NotSupportedException("AsType is not supported on a slice view. Please clone the view first.");
        }
        public void Clear()
        {
            indicies.Clear();
        }
        public Series<DataType> Clone()
        {
            List<DataType> newData = new List<DataType>();
            foreach (int index in indicies)
            {
                newData.Add(original[index]);  // Copy từng phần tử từ original
            }

            // Tạo một Series mới hoàn toàn từ dữ liệu đã copy
            return new Series<DataType>(original.Name, newData);
        }
        public IList<int> Find(DataType item)
        {
            return indicies.Where(index => Equals(item, original[index])).ToList();
        }
    }

    public class Series<DataType> : ICollection<DataType>, ISeries<DataType> where DataType : notnull
    {
        private string? name;
        private IList<DataType> values;
        private Dictionary<object, int> indexMap;
        // support method to check type is valid to add the data series
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : this.dType.IsAssignableFrom(value.GetType());
        }

        //constructor
        public Series(string? name, IList<DataType> values, IList<object>? index = null)
        {

            this.name = name;
            this.values = new List<DataType>(values);
            
            // handle index
            indexMap = new Dictionary<object, int>();
            if (index == null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    indexMap[i] = i;
                }
            }
            else
            {
                if (index.Count != values.Count)
                {
                    throw new ArgumentException("index size must be same as the value size");
                }
                for (int i = 0; i < index.Count; i++)
                {
                    indexMap[index[i]] = i;
                }
            }

        }
        public Series(Series<DataType> other)
        {
            SupportMethods.CheckNull(other);
            this.name = other.name;
            this.values = new List<DataType>((IEnumerable<DataType>)other.Values);
            this.indexMap = new Dictionary<object, int>(other.indexMap);
        }
        public Series(Tuple<string, IList<DataType>> KeyAndValues)
        {
            SupportMethods.CheckNull(KeyAndValues);
            this.name = KeyAndValues.Item1;
            if (KeyAndValues.Item2 == null)
            {
                this.values = new List<DataType>();
            }
            else
            {
                this.values = new List<DataType>(KeyAndValues.Item2);
            }

            indexMap = new Dictionary<object, int>();
            for (int i = 0; i < values.Count; i++)
            {
                indexMap[i] = i;
            }
        }

        // utility
        public string? Name { get { return this.name; } }
        public int Count => values.Count;
        public bool IsReadOnly => false;
        public DataType this[int index]
        {
            get
            {
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                if (values == null)
                {
                    throw new ArgumentException("list of values is null");
                }
                return values[index];
            }
            set
            {
                SupportMethods.CheckNull(values);
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                values[index] = (DataType)value;
            }
        }
        public Type dType => typeof(DataType);
        public IReadOnlyList<DataType> Values => values.AsReadOnly();

        // iterator
        public IEnumerator<DataType> GetEnumerator()
        {
            return values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        //methods begin here 
        // modify the series
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
        public void ChangeItem(int index, DataType item)
        {
            if (index < 0 || index >= values.Count)
            {
                throw new ArgumentOutOfRangeException("index is out of range");
            }
            this.values[index] = item;
        }
        // accessing the series
        public IList<DataType> GetItem(int indexFrom, int indexTo, int step = 1)
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
            if (indexTo == indexFrom)
            {
                return result;  // Trả về danh sách rỗng theo phong cách Python
            }
            if (step > 0)
            {
                for (int i = indexFrom; i <= indexTo; i += step)
                {
                    result.Add(values[i]);
                }
            }
            else
            {
                for (int i = indexFrom; i >= indexTo; i += step)
                {
                    result.Add(values[i]);
                }
            }
            return result;
        }
        public SeriesSliceView<DataType> View(ValueTuple<int, int, int> slices)
        {
            if (slices.Item1 < 0 || slices.Item2 < 0)
            {
                throw new ArgumentException("index start or and is out of range");
            }
            return new SeriesSliceView<DataType>(this, (slices.Item1, slices.Item2, slices.Item3));
        }
        public ISeries<DataType> Head(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<DataType> items = new List<DataType>();
            for (int i = 0; i < count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series<DataType>(this.name, items);
        }
        public ISeries<DataType> Tail(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<DataType> items = new List<DataType>();
            for (int i = this.Count - count; i < this.Count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series<DataType>(name, items);
        }

        // searching and filter
        public IList<DataType> Filter(Func<DataType, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }
        public bool Contains(DataType item)
        {
            return values.Contains(item);
        }
        public IList<int> Find(DataType? item)
        {
            IList<int> Indexes = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<DataType>.Default.Equals(values[i], item))
                    Indexes.Add(i);
            }
            return Indexes;
        }
        public IList<int> Find(object? item)
        {
            return this.Find((DataType?)item);
        }

        // utility method
        public ISeries AsType(Type NewType, bool ForceCast = false)
        {
            SupportMethods.CheckNull(NewType);

            // generate right type of list
            var listType = typeof(List<>).MakeGenericType(NewType);
            var newValues = (IList)Activator.CreateInstance(listType)!;
            object? defaultValue = NewType.IsValueType ? Activator.CreateInstance(NewType) : null;

            foreach (var v in values)
            {
                //handle null and DBNull.Value case
                if (v == null)
                {
                    newValues.Add(defaultValue);
                    continue;
                }
                if (v is object obj && obj == DBNull.Value)
                {
                    newValues.Add(defaultValue);
                    continue;
                }

                // handle the case if the v is not null
                try
                {
                    object convertedValue;
                    if (NewType == typeof(DateTime) && v is string str)
                    {
                        convertedValue = DateTime.TryParse(str, out DateTime dt) ? dt :
                                        (ForceCast ? throw new InvalidCastException($"Cannot convert {str} to DateTime") : (DateTime)defaultValue!);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(v, NewType);
                    }

                    newValues.Add(convertedValue);
                }
                catch
                {
                    if (ForceCast)
                        throw new InvalidCastException($"Cannot convert {v} to {NewType}");
                    else
                        newValues.Add(DBNull.Value);
                }
            }

            return (ISeries)Activator.CreateInstance(typeof(Series<>).MakeGenericType(NewType), this.Name, newValues)!;
        }
        public void Sort(Comparer<DataType>? comparer = null)
        {
            if (values == null) return;
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

        // copy
        public Series<DataType> Clone()
        {
            if (this.values == null)
            {
                return new Series<DataType>(this.name, new List<DataType>());
            }
            return new Series<DataType>(this.name, new List<DataType>(values));
        }
        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.CopyTo((DataType[])array, arrayIndex);
        }
        // methods end
    }
}
