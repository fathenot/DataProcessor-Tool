using System.Collections;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataProcessor
{
    public interface ISeries
    {
        string? Name { get; }
        IReadOnlyList<object?> Values { get; }
        Type dType { get;}
        bool IsReadOnly { get; }
        void Clear();
        public bool Remove(object? item);
        void Add(object? item);
        public bool IsValidType(object? value);
        public int Count { get; }
        public ISeries Clone();
        public ISeries AsType(Type NewType, bool ForceCast = false);
        public IList<int> Find(object? item);
        public object? this[int index] { get; set; }
        public ISeries View(ValueTuple<int, int, int> slice);
    }

    public class SeriesSliceView :ISeries, IEnumerable
    {
        private ISeries _original;
        private readonly int _start;
        private readonly int _end;
        private readonly int _step;
        private readonly List<int> indices;

        public SeriesSliceView(ISeries original, int start, int end, int step)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));
            if (step == 0) throw new ArgumentException("step must not be zero", nameof(step));
            _start = start;
            _end = end;
            _step = step;

            // set indices
            indices = new List<int>();
            if (step > 0)
            {
                for (int i = start; i <= end && i < _original.Count; i += step)
                {
                    indices.Add(i);
                }
            }
            else
            {
                for (int i = start; i >= end && i >= 0; i += step)
                {
                    indices.Add(i);
                }
            }
        }

        // utilities
        public IReadOnlyList<object?> Values
        {
            get
            {
                // Trả về một danh sách giá trị tham chiếu tới giá trị gốc theo slice
                return indices.Select(idx => _original[idx]).ToList();
            }
        }
        public Type dType => _original.dType;
        public bool IsReadOnly => _original.IsReadOnly;
        public int Count => indices.Count;
        public string? Name => _original.Name;
        public object? this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _original[indices[index]];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _original[indices[index]] = value;
            }
        }

        // methods begin here
        public bool IsValidType(object? item)
        {
            return this._original.IsValidType(item);
        }
        // modify the view
        public void Clear()
        {
            indices.Clear();
        }
        public ISeries AsType(Type NewType, bool ForceCast = false)
        {
            throw new NotSupportedException("AsType is not supported on a slice view. Please clone the view first.");
        }
        public ISeries Clone() => _original.Clone().View((_start, _end, _step));      
        public ISeries View(ValueTuple<int, int,int> slice)
        {
            int start = slice.Item1;
            int end = slice.Item2;
            int step = slice.Item3;
            return new SeriesSliceView(this, start, end, step);
        }

        //removal the mask of the view. These methods doesn't change the _original
        public void RemoveAt(int index)
        {
            if(index < 0 || index >= indices.Count)
            {
                throw new ArgumentOutOfRangeException("index is out of range", nameof(index));
            }
            indices.RemoveAt(index);
        }
        public bool Remove(object? item)
        {
            int removed = indices.RemoveAll(idx => Equals(_original[idx], item));
            return removed != 0;
        }

        public void Add(object? item)
        {
            throw new NotSupportedException("Add is not supported in slice view please clone the view first");
        }
        public IList<int> Find(object? item)
        {
            IList<int> Indexes = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }

        //iterator
        public IEnumerator<object?> GetEnumerator()
        {
            foreach (int idx in indices)
            {
                yield return _original[idx];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public class Series : ISeries, ICollection<object>, IEnumerable
    {
        private string? name;
        private IList<object>? values;
        private Type dtype;

        // support method to check type is valid to add the data series
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : this.dType.IsAssignableFrom(value.GetType());
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
                dtype = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            }
            else
            {
                this.dtype = typeof(object);
            }

        }       
        public Series(Series other)
        {
            SupportMethods.CheckNull(other);
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
        public Series(ValueTuple<string, IList<object>> NameAndValues)
        {
            SupportMethods.CheckNull(NameAndValues);
            this.name = NameAndValues.Item1;
            this.values = new List<object>(NameAndValues.Item2);
            var tempValuesList = NameAndValues.Item2;
            var nonNullTypes = tempValuesList.Where(v => v != null).Select(v => v.GetType()).Distinct();
            dtype = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            this.dtype = dType;
        }

        // Properties
        public string? Name { get { return this.name; } }
        public IReadOnlyList<object?> Values => values == null ? throw new Exception("values list is null") : values as IReadOnlyList<object?> ?? values.ToList();
        public int Count => values == null ? 0 : values.Count;
        public bool IsReadOnly { get { return false; } }
        public object this[int index]
        {
            get
            {
                SupportMethods.CheckNull(this.values);
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                return values[index];
            }
            set
            {
                SupportMethods.CheckNull(this.values);
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
            SupportMethods.CheckNull(this.values);
            return values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            SupportMethods.CheckNull(this.values);
            return values.GetEnumerator();
        }

        //methods begin here
        //modify the list
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
        public bool Remove(object? item)
        {
            SupportMethods.CheckNull(this.values);
            return values.Remove(item);
        }
        public void Clear()
        {
            if (values != null) values.Clear();
        }
        public void ChangeItem(int index, object? item)
        {
            if (values == null)
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

        // Access the series
        public IList<object> GetItem(int indexFrom, int indexTo, int step = 1)
        {
            IList<object> result = new List<object>();

            // check valid argument
            SupportMethods.CheckNull(this.values);
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
        public ISeries Head(int count)
        {
            if(count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if(this.values == null)
            {
                return new Series(this.name, null);
            }
            List<object> items = new List<object>();
            for(int i = 0; i< count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series(this.name, items);
        }
        public ISeries Tail(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (this.values == null)
            {
                return new Series(this.name, null);
            }
            List<object> items = new List<object>();
            for (int i = this.Count - count; i < this.Count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series(name, items);
        }
        public ISeries View(ValueTuple<int, int, int>slices)
        {
            if(slices.Item1 < 0 || slices.Item2 < 0)
            {
                throw new ArgumentException("index start or and is out of range");
            }
            return new SeriesSliceView(this, slices.Item1, slices.Item2, slices.Item3);         
        }
        
        // utility methods
        public ISeries AsType(Type NewType, bool ForceCast = false)
        {
            if (NewType == null) throw new ArgumentNullException(nameof(NewType));

            if (this.values == null)
            {
                return new Series(this.Name, null);
            }

            var newValues = new List<object>(values.Count);
            // add new value to newValues to create new Series
            foreach (var v in values)
            {
                if (v == null || v == DBNull.Value)
                {
                    newValues.Add(DBNull.Value);
                    continue;
                }

                try
                {
                    if (NewType == typeof(DateTime) && v is string str)
                    {
                        newValues.Add(DateTime.TryParse(str, out DateTime dt) ? dt :
                                      (ForceCast ? throw new InvalidCastException($"Cannot convert {str} to DateTime") : DBNull.Value));
                    }
                    else
                    {
                        newValues.Add(Convert.ChangeType(v, NewType));
                    }
                }
                catch
                {
                    if (ForceCast)
                        throw new InvalidCastException($"Cannot convert {v} to {NewType}");
                    else
                        newValues.Add(DBNull.Value);
                }
            }

            return new Series(this.Name, newValues);
        }
        public void Sort(Comparer<object?>? comparer = null)
        {
            SupportMethods.CheckNull(this.values);
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

        // searching and filter
        public IList<object> Filter(Func<object?, bool> filter)
        {
            if(values == null)
            {
                throw new Exception($"Values list is null");
            }
            return values.AsEnumerable().Where(filter).ToList();
        }       
        public IList<int> Find(object? item) // find all occurance of item
        {
            IList<int> Indexes = new List<int>();
            for(int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }
        public bool Contains(object item)
        {
            return values == null ? false : values.Contains(item);
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
        // clone
        public ISeries Clone()
        {
            if(this.values == null)
            {
                return new Series(this.name, null);
            }
            return new Series(this.name, new List<object>(this.values));
        }    
        public void CopyTo(object[] array, int arrayIndex)
        {
            if (values == null)
            {
                return;
            }
            values.CopyTo((object[])array, arrayIndex);
        }
        // methods end here
    }


    public class Series<DataType> : ICollection<DataType>, ISeries
    {
        private string name;
        private IList<DataType> values;
        // support method to check type is valid to add the data series
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : this.dType.IsAssignableFrom(value.GetType());
        }

        //constructor
        public Series(string name, IList<DataType> values)
        {
            
            this.name = name;
            this.values = values;
            if (values == null)
            {
                this.values = new List<DataType>();
            }
        }
        public Series(Series <DataType> other)
        {
            SupportMethods.CheckNull(other);
            this.name = other.name;
            this.values = new List<DataType>((IEnumerable<DataType>)other.Values);
        }
        public Series(Tuple<string, IList<DataType>> KeyAndValues)
        {
            SupportMethods.CheckNull(KeyAndValues);
            this.name = KeyAndValues.Item1;
            if(KeyAndValues.Item2 == null)
            {
                this.values = new List<DataType>();
            }
            else
            {
                this.values = new List<DataType>(KeyAndValues.Item2);
            }           
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
                SupportMethods.CheckNull(values);
                if (index < 0 || index >= values.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                values[index] = (DataType)value;
            }
        }
        public Type dType => typeof(DataType);
        public IReadOnlyList<object?> Values => values?.Select(v => (object?)v).ToList() ?? new List<object?>();

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
        public bool Remove(object? item)
        {
            return values.Remove((DataType)item);
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
            for (int i = indexFrom; i != indexTo; i += step)
            {
                result.Add(values[i]);
            }
            return result;
        }
        public ISeries View(ValueTuple<int, int, int> slices)
        {
            if (slices.Item1 < 0 || slices.Item2 < 0)
            {
                throw new ArgumentException("index start or and is out of range");
            }
            return new SeriesSliceView(this, slices.Item1, slices.Item2, slices.Item3);
        }
        public ISeries Head(int count)
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
        public ISeries Tail(int count)
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
        public ISeries Clone()
        {
            if (this.values == null)
            {
                return new Series(this.name, null);
            }
            return new Series<DataType>(this.name, new List<DataType>(values) );
        }
        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.CopyTo((DataType[])array, arrayIndex);
        }
        // methods end
    }

}
