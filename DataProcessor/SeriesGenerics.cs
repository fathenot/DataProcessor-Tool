using System.Collections;
using System.Text;

namespace DataProcessor
{
    public class Series<DataType> : ISeries<DataType> where DataType : notnull
    {
        private string? name;
        private List<DataType> values;
        private Dictionary<object, List<int>> indexMap;
        private List<object> index;
        private bool defaultIndex = false;
        // support method to check type is valid to add the data series

        public class View
        {
            private Series<DataType> series;
            private List<object> index;
            Supporter.OrderedSet<int> convertedToIntIdx = new Supporter.OrderedSet<int>();
        
            public View(Series<DataType> series, List<object> index)
            {
                if (series == null || index == null) throw new ArgumentNullException();

                this.series = series;
                this.index = new List<object>();

                // Kiểm tra nhãn có tồn tại không
                if (index.Any(v => !this.series.indexMap.ContainsKey(v)))
                    throw new IndexOutOfRangeException("Index out of range");

                // Kiểm tra số lượng mỗi nhãn
                Dictionary<object, int> indexCount = new Dictionary<object, int>();
                foreach (var i in index)
                {
                    if (!indexCount.ContainsKey(i))
                        indexCount[i] = 0;
                    indexCount[i]++;
                }

                foreach (var pair in indexCount)
                {
                    if (pair.Value > this.series.indexMap[pair.Key].Count)
                        throw new ArgumentException($"Label {pair.Key} requested {pair.Value} times, but only {this.series.indexMap[pair.Key].Count} available");
                }

                // Nếu hợp lệ, gán index
                this.index = new List<object>(index);
            }


            public View(Series<DataType> series, (object start, object end, int step) slice)
            {
                if (series == null) throw new ArgumentNullException();
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                this.index = new List<object>();
                this.series = series;
                if (!series.indexMap.TryGetValue(slice.start, out var startList))
                    throw new ArgumentException("start is not exist");

                if (!series.indexMap.TryGetValue(slice.end, out var endList))
                    throw new ArgumentException("end is not exist");


                List<ValueTuple<int, int>> position = new List<(int, int)>();
                for (int i = 0; i < Math.Min(startList.Count, endList.Count); i++)
                {
                    position.Add((startList[i], endList[i]));
                }

                // generate index
                foreach (var pair in position)
                {
                    if (slice.step > 0)
                    {
                        for (int i = pair.Item1; i <= pair.Item2; i += slice.step)
                        {
                            if (this.convertedToIntIdx.Add(i)) index.Add(series.index[i]);
                        }
                    }
                    if (slice.step < 0)
                    {
                        for (int i = pair.Item1; i >= pair.Item2; i += slice.step)
                        {
                            if (this.convertedToIntIdx.Add(i)) index.Add(series.index[i]);
                        }
                    }

                }
            }

            public View GetView(List<object> subIndex)
            {
                if (subIndex.Any(v => !this.index.Contains(v)))
                    throw new ArgumentOutOfRangeException("Sub-index contains values not in the current View");

                return new View(this.series, subIndex);
            }
            public Series<DataType> ToSeries(string? name = null)
            {
                List<DataType> newValues = new List<DataType>(this.index.Count);
                List<object> newIndex = new List<object>(this.index.Count);

                foreach (var idx in this.index)
                {
                    if (this.series.indexMap.TryGetValue(idx, out List<int>? positions))
                    {
                        foreach (var pos in positions)
                        {
                            if (this.convertedToIntIdx.Contains(pos))
                            {
                                newValues.Add(this.series.Values[pos]);
                                newIndex.Add(idx);
                            }                         
                        }
                    }
                }

                return new Series<DataType>(name, newValues, newIndex);
            }

            public void UpdateValue(object idx, DataType newValue)
            {
                if (!index.Contains(idx))
                    throw new KeyNotFoundException("Index not in View");

                // Chỉ cập nhật những phần tử thuộc View
                this.series = ToSeries(this.series.name);
                foreach (var pos in series.indexMap[idx])
                {
                        series.values[pos] = newValue;
                }
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

            public IEnumerator<DataType> GetValueEnumerator()
            {
                foreach (var idx in this.index)
                {
                    if (this.series.indexMap.TryGetValue(idx, out var positions))
                    {
                        foreach (var pos in positions)
                        {
                            if (this.convertedToIntIdx.Contains(pos))
                                yield return this.series.Values[pos];
                        }
                    }
                }
            }

            public IEnumerator<object> GetIndexEnumerator()
            {
                foreach (var idx in this.index)
                {
                    yield return idx;
                }
            }
        }

        public bool IsValidType(object? value)
        {
            return value == null || value == DBNull.Value ? true : this.dType.IsAssignableFrom(value.GetType());
        }

        //constructor
        
        public Series(string? name, List<DataType> values, IList<object>? index = null)
        {
            this.name = name;
            if(values == null)
            {
                this.values = new List<DataType>();
            }
            else
            {
                this.values = new List<DataType>(values);
            }

                // handle index
            indexMap = new Dictionary<object, List<int>>();         
            if (index == null)
            {
                this.index = new List<object>();
                for (int i = 0; i < this.values.Count; i++)
                {
                    indexMap[i] = new List<int> { i };
                    this.index.Add(i);
                }
            }
            else
            {
                if (index.Count != this.values.Count)
                {
                    throw new ArgumentException("index size must be same as the value size");
                }
                for (int i = 0; i < index.Count; i++)
                {
                    var key = index[i];
                    if (!indexMap.TryGetValue(key, out List<int>? value))
                    {
                        value = new List<int>();
                        indexMap[key] = value;
                    }

                    value.Add(i);
                }
                this.index = new List<object>(index);
            }

        }
        public Series(Series<DataType> other)
        {
            Supporter.CheckNull(other);
            this.name = other.name;
            this.values = new List<DataType>((IEnumerable<DataType>)other.Values);
            this.indexMap = new Dictionary<object, List<int>>(other.indexMap);
            this.index = new List<object>(other.index);
        }
        public Series(Tuple<string?, IList<DataType>> KeyAndValues)
        {
            Supporter.CheckNull(KeyAndValues);
            this.name = KeyAndValues.Item1;
            if (KeyAndValues.Item2 == null)
            {
                this.values = new List<DataType>();
            }
            else
            {
                this.values = new List<DataType>(KeyAndValues.Item2);
            }

            indexMap = new Dictionary<object, List<int>>();
            this.index = Enumerable.Range(0, values.Count - 1).Cast<object>().ToList();
            for (int i = 0; i < values.Count; i++)
            {
                indexMap[i] = new List<int> { i };
            }
        }

        // utility
        public string? Name { get { return this.name; } }
        public int Count => values.Count;
        public bool IsReadOnly => false;
        public List<DataType> this[object index]
        {
            get
            {
                if (!this.indexMap.TryGetValue(index, out List<int>? indexNum))
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                List<DataType> res = new List<DataType>();
                foreach(var idx in indexNum)
                {
                    res.Add(this.values[idx]);
                }
                return res;
            }
        }
        public Type dType => typeof(DataType);
        public IReadOnlyList<DataType> Values => values.AsReadOnly();
        public IReadOnlyList<object> Index => this.index;

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
        public void UpdateValues(object index, List<DataType> newValues)
        {
            if (!this.index.Contains(index))
            {
                throw new ArgumentException("Index not found", nameof(index));
            }

            var indices = this.indexMap[index]; // Lấy danh sách index ánh xạ

            // Đảm bảo newValues hợp lệ: hoặc có 1 phần tử hoặc có đúng số lượng phần tử của index
            if (newValues.Count != 1 && newValues.Count != indices.Count)
            {
                throw new ArgumentException("Value list must either have one element or match the indexed element count.");
            }

            for (int j = 0; j < indices.Count; j++)
            {
                this.values[indices[j]] = (newValues.Count == 1) ? newValues[0] : newValues[j];
            }
        }
        public void Add(DataType item, object? index = null)
        {
            if(index == null)
            {
                if (!this.defaultIndex)
                {
                    throw new ArgumentException("Cannot add null index when index is not default");
                }
                this.index.Add(this.Count);
                this.indexMap[this.Count] = new List<int> { this.Count };
                return;
            }
            if (index != null)
            {
                this.index.Add(index);
                if (!indexMap.TryGetValue(index, out var list))
                {
                    list = new List<int>();
                    indexMap[index] = list;
                }
                list.Add(values.Count);
                this.defaultIndex = false;
            }
            this.values.Add(item);
        }
        public bool Remove(DataType item, bool deleteIndexIfEmpty = true)
        {
            bool removed = false;
            var keysToDelete = new List<object>(); // Lưu index cần xóa
            foreach (var key in indexMap.Keys.ToList()) // ToList() để tránh Collection Modified
            {
                if (indexMap.TryGetValue(key, out var positions))
                {
                    var toRemove = positions.Where(i => Equals(values[i], item)).ToList();
                    if (toRemove.Count > 0)
                    {
                        removed = true;
                        positions.RemoveAll(i => toRemove.Contains(i));

                        if (positions.Count == 0 && deleteIndexIfEmpty)
                        {
                            keysToDelete.Add(key);
                        }
                    }
                }
            }

            if(!removed) return false;

            // Cleanup values và cập nhật lại indexMap
            var newValues = new List<DataType>();
            var newIndexMap = new Dictionary<object, List<int>>();

            int[] indexMapping = new int[values.Count]; // Ánh xạ index cũ -> index mới
            int newIdx = 0;

            for (int i = 0; i < values.Count; i++)
            {
                if (!indexMap.Values.Any(lst => lst.Contains(i))) continue; // Bỏ qua giá trị không còn được tham chiếu

                newValues.Add(values[i]);
                indexMapping[i] = newIdx++;

                foreach (var key in indexMap.Keys)
                {
                    if (indexMap[key].Contains(i))
                    {
                        if (!newIndexMap.ContainsKey(key))
                        {
                            newIndexMap[key] = new List<int>();
                        }
                        newIndexMap[key].Add(indexMapping[i]);
                    }
                }
            }

            values = newValues;
            indexMap = newIndexMap;

            // Xóa index sau khi xử lý xong để tránh Collection Modified
            foreach (var key in keysToDelete)
            {
                indexMap.Remove(key);
            }

            return true;
        }
        public void Clear()
        {
            values.Clear();
            this.index.Clear();
            this.indexMap.Clear();
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
        public Series<DataType> Head(int count)
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
        public Series<DataType> Tail(int count)
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
        public IList<int> Find(DataType item)
        {
            IList<int> Indexes = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<DataType>.Default.Equals(values[i], item))
                    Indexes.Add(i);
            }
            return Indexes;
        }

        // utility method
        public ISeries AsType(Type NewType, bool ForceCast = false)
        {
            Supporter.CheckNull(NewType);

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
