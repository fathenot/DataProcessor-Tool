using System.Collections;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using NUnit.Framework.Constraints;
using System.Text;

namespace DataProcessor
{
    public class Series : ISeries
    {
        private string? name;
        private List<object?> values;
        private Dictionary<object, List<int>> indexMap;
        private List<object> index;
        private Type dtype;
        private readonly bool defaultIndex;
        // inner class
        public class View
        {
            private List<object> index;
            private Series series;
            Supporter.OrderedSet<int> convertedToIntIdx = new Supporter.OrderedSet<int>();
            public View(Series series,List<object> indices)
            {
                if (series == null || indices  == null) throw new ArgumentNullException();
                this.index = new List<object>();
                this.series = series;// tham chiếu tới series;
                // check valid 
                bool Invalid = indices.Any(v => !this.series.indexMap.Keys.Contains(v));
                if (Invalid) throw new IndexOutOfRangeException("index out of range");
                foreach(var index in indices)
                {
                    this.index.Add(index);
                }
            }
            public View(Series series, (object start, object end, int step) slice)
            {
                if (series == null) throw new ArgumentNullException();
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                this.index = new List<object>();
                this.series = series;
                if (!series.index.Contains(slice.start)) { throw new ArgumentException("start is not exist"); }
                if (!series.index.Contains(slice.end)) { throw new ArgumentException("end is not exist"); }

                List<ValueTuple<int, int>> position = new List<(int, int)>();
                for (int i = 0; i < Math.Min(series.indexMap[slice.start].Count, series.indexMap[slice.end].Count); i++)
                {
                    position.Add((series.indexMap[slice.start][i], series.indexMap[slice.end][i]));
                }

                // generate index
                foreach (var pair in position)
                {
                    if(slice.step > 0)
                    {
                        for (int i = pair.Item1; i <= pair.Item2; i += slice.step)
                        {
                            if (this.convertedToIntIdx.Add(i)) index.Add(series.index[i]);
                        }
                    }
                    if(slice.step < 0)
                    {
                        for (int i = pair.Item1; i >= pair.Item2; i += slice.step)
                        {
                            if (this.convertedToIntIdx.Add(i)) index.Add(series.index[i]);
                        }
                    }
                    
                }

            }
            public ISeries Clone()
            {
                List<object?> data = new List<object?>();
                foreach(var i in this.convertedToIntIdx)
                {
                    data.Add(series.values[i]);
                }
                var res = new Series(this.series.name, data, series.index);
                return res.AsType(series.dType);
            }

            public View GetView((object start, object end, int step) slice) // this just change view of the current vỉew
            {
                if (series == null) throw new ArgumentNullException();
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                if (!this.index.Contains(slice.start)) { throw new ArgumentException("start is not exist"); }
                if (!this.index.Contains(slice.end)) { throw new ArgumentException("end is not exist"); }

                List<object> newIndex = new List<object>();
                Supporter.OrderedSet<int> NewConvertedToIntIdx = new Supporter.OrderedSet<int>();
                
                // find start
                var startIndex = this.index
                                .Select((value, index) => (value, index))  // Đính kèm index vào từng phần tử
                                .Where(pair => Equals(pair.value, slice.start))       // Lọc ra các phần tử có giá trị bằng target
                                .Select(pair => pair.index)                // Lấy index của các phần tử đó
                                .ToList();
                var endIndex = this.index
                              .Select((value, index) => (value, index))  // Đính kèm index vào từng phần tử
                              .Where(pair => Equals(pair.value, slice.end))       // Lọc ra các phần tử có giá trị bằng target
                              .Select(pair => pair.index)                // Lấy index của các phần tử đó
                              .ToList();
                for (int i = 0; i < Math.Min(startIndex.Count, endIndex.Count); i++)
                {
                    int startPos = startIndex[i];
                    int endPos = endIndex[i];

                    if (slice.step > 0)
                    {
                        for (int j = startPos; j <= endPos; j += slice.step)
                        {
                            if (NewConvertedToIntIdx.Add(j))
                                newIndex.Add(this.index[j]);
                        }
                    }
                    else
                    {
                        for (int j = startPos; j >= endPos; j += slice.step)
                        {
                            if (NewConvertedToIntIdx.Add(j))
                                newIndex.Add(this.index[j]);
                        }
                    }
                }

                this.index = newIndex;
                this.convertedToIntIdx = NewConvertedToIntIdx;
                GC.Collect();
                return this;
            }

        }

        // support method to check type is valid to add the data series
        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : this.dType.IsAssignableFrom(value.GetType());
        }

        //constructor
        public Series(string? name, List<object?> values, List<object>? index = null)
        {
            this.name = name;
            this.values = values;
            if(values != null)// infer datatype
            {
                this.values = new List<object?>(values);
                var nonNullTypes = values.Where(v => v != null).Select(v => v!.GetType()).Distinct();
                dtype = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            }
            else
            {
                values = new List<object?>();
                this.dtype = typeof(object);
            }

            // handle index
            indexMap = new Dictionary<object, List<int>>();
            if (index == null)
            {
                this.defaultIndex = true;
                this.index = Enumerable.Range(0, values.Count-1).Cast<object>().ToList();
                for (int i = 0; i < values.Count; i++)
                {
                    indexMap[i] = new List<int> { i };
                }
            }
            else
            {
                this.defaultIndex = false;
                this.index = new List<object>(index);
                if(index.Count != values.Count)
                {
                    throw new ArgumentException("index size must be same as the value size");
                }
                for (int i = 0; i < index.Count; i++)
                {
                    var key = index[i];
                    if (!indexMap.ContainsKey(key))
                    {
                        indexMap[key] = new List<int>();
                    }
                    indexMap[key].Add(i);
                }

            }

        }       
        public Series(Series other)
        {
            Supporter.CheckNull(other);
            this.name = other.name;
            this.values = new List<object?>(other.values);
            this.dtype = other.dType;
            this.indexMap = new Dictionary<object, List<int>>(other.indexMap);
            this.index = new List<object>(other.index);
        }
        public Series(ValueTuple<string, List<object>> NameAndValues)
        {
            Supporter.CheckNull(NameAndValues);
            this.name = NameAndValues.Item1;
            this.values = new List<object?>(NameAndValues.Item2);
            var tempValuesList = NameAndValues.Item2;
            var nonNullTypes = tempValuesList.Where(v => v != null).Select(v => v.GetType()).Distinct();
            dtype = nonNullTypes.Count() == 1 ? nonNullTypes.First() : typeof(object);
            this.dtype = dType;

            indexMap = new Dictionary<object, List<int>>();
            index = Enumerable.Range(0, values.Count - 1).Cast<object>().ToList();
            for (int i = 0; i < values.Count; i++)
            {
                indexMap[i] = new List<int> { i };
            }
        }

        // Properties
        public string? Name { get { return this.name; } }
        public IReadOnlyList<object?> Values => values == null ? throw new Exception("values list is null") : values as IReadOnlyList<object?> ?? values.ToList();
        public int Count => values == null ? 0 : values.Count;
        public bool IsReadOnly { get { return false; } }
        public Type dType
        {
            get => dtype;
            private set => dtype = value;
        }
        public List<object?> this[object index]
        {
            get
            {
                if (!this.Contains(index))
                {
                    throw new ArgumentException("index not found", nameof(index));
                }
                List<object?> res = new List<object?>();
                foreach (int i in this.indexMap[index])
                {
                    res.Add(this.values[i]);
                }
                return res;
            }
        }
        public IReadOnlyList<object> Index => this.index;

        // iterator
        public IEnumerator<object?> GetEnumerator()
        {
            return this.values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        //methods begin here
        //modify the list
        public void Add(object? item, object? index  = null)
        {
            if (!IsValidType(item))
            {
                throw new ArgumentException($"Expected type {dType}, but got {item?.GetType()}. You must change the đata type to object first");
            }
            if(index == null)
            {
                if (!defaultIndex)
                {
                    throw new ArgumentException("Cannot add null index when index is not default");
                }
                this.index.Add(this.Count);
                this.indexMap[this.Count] = new List<int> { this.Count };
            }
            if (index != null)
            {
                this.index.Add(index);
                if (!indexMap.TryGetValue(index, out var list))
                {
                    list = new List<int>();
                    indexMap[index] = list;
                }
                list.Add(values.Count - 1);
            }
            this.values.Add(item);
        }

        public bool Remove(object? item)
        {
            if(this.values == null) { return false; }
            return values.Remove(item);
        }
        public void Clear()
        {
            this.values.Clear();
            this.index.Clear();
            this.indexMap.Clear(); 
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
       
        // access the series
        public ISeries Head(int count)
        {
            if(count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<object?> items = new List<object?>();
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
            List<object?> items = new List<object?>();
            for (int i = this.Count - count; i < this.Count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series(name, items);
        }
        public View GetView((object start, object end, int step) slices)
        {
            // check valid start, end
            if(indexMap.ContainsKey(slices.start) || !indexMap.ContainsKey(slices.end))
            {
                throw new ArgumentException("start or end is not exist");
            }
            return new View(this, slices);
        }
        
        // utility methods
        public ISeries AsType(Type NewType, bool ForceCast = false)
        {
            if (NewType == null) throw new ArgumentNullException(nameof(NewType));

            var newValues = new List<object?>(values.Count);
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
                    if (NewType.IsEnum)
                    {
                        if (v is string strEnum && Enum.TryParse(NewType, strEnum, true, out object? enumValue))
                        {
                            newValues.Add(enumValue);
                        }
                        else if (v is int intEnum && Enum.IsDefined(NewType, intEnum))
                        {
                            newValues.Add(Enum.ToObject(NewType, intEnum));
                        }
                        else
                        {
                            if (ForceCast) throw new InvalidCastException($"Cannot convert {v} to {NewType}");
                            newValues.Add(DBNull.Value);
                        }
                    }

                    else if (NewType == typeof(DateTime) && v is string str)
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

            return new Series(this.Name, newValues, this.index);
        }
        public void Sort(Comparer<object?>? comparer = null)
        {
            if(values == null) { return; }
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
        public IList<object?> Filter(Func<object?, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }
        
        public List<int> Find(object? item) // find all occurance of item
        {
            List<int> Indexes = new List<int>();
            for(int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }

        public bool Contains(object? item)
        {
            return values == null ? false : values.Contains(item);
        }

        // show the series
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Series: {Name ?? "Unnamed"}"); // In tên Series
            sb.AppendLine("Index | Value");
            sb.AppendLine("--------------");
            for (int i = 0; i < values.Count; i++)
            {
                sb.AppendLine($"{index[i].ToString(),5} | {values[i]?.ToString() ?? "null"}");
            }
            return sb.ToString();
        } 
        // clone
        public ISeries Clone()
        {
            return new Series(this.name, this.values);
        }    
        public void CopyTo(object?[] array, int arrayIndex)
        {
            if (values == null)
            {
                return;
            }
            values.CopyTo((object?[])array, arrayIndex);
        }

        public Series<DataType> ConvertToGenerics<DataType>() where DataType : notnull
        {
            var newValues = new List<DataType>(values.Count);
            foreach (var v in values)
            {
                if (v == null || v == DBNull.Value)
                {
                    newValues.Add(default!); // Giá trị mặc định của T
                    continue;
                }

                try
                {
                    // Nếu v đã là DataType, thêm vào luôn
                    if (v is DataType castedValue)
                    {
                        newValues.Add(castedValue);
                    }
                    // Xử lý chuyển đổi kiểu dữ liệu
                    else
                    {
                        object convertedValue = Convert.ChangeType(v, typeof(DataType));
                        newValues.Add((DataType)convertedValue);
                    }
                }
                catch
                {
                    newValues.Add(default!);
                }
            }
            return new Series<DataType>(this.name, newValues, this.index);
        }
        // methods end here
        
    }
}
