using System.Collections;
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
        private bool defaultIndex;

        // handle multi thread this will be implement in the future
        private readonly Semaphore writeSemaphore = new Semaphore(1, 1);
        private ReaderWriterLock rwl = new ReaderWriterLock();

        // inner class
        public class View
        {
            private List<object> index;
            private Series series;
            List<int> convertedToIntIdx = new List<int>();

            public View(Series series, List<object> indices)
            {
                ArgumentNullException.ThrowIfNull(series);
                ArgumentNullException.ThrowIfNull(indices);
                this.index = new List<object>();
                this.series = series;
                foreach (var index in indices)
                {
                    if (!series.indexMap.TryGetValue(index, out var positions))
                    {
                        throw new IndexOutOfRangeException($"Index {index} is out of range");
                    }

                    this.index.Add(index);
                    this.convertedToIntIdx.AddRange(positions); // Tránh lặp nhiều lần
                }
            }
            public View(Series series, (object start, object end, int step) slice)
            {
                //check valid argument
                if (series == null) throw new ArgumentNullException();
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                if (!series.index.Contains(slice.start)) { throw new ArgumentException($"start index {slice.start} does not exist"); }
                if (!series.index.Contains(slice.end)) { throw new ArgumentException($"end index {slice.end} does not exist"); }

                // main method logic
                Supporter.OrderedSet<int> removedDuplicatedIdx = new Supporter.OrderedSet<int>();
                this.index = new List<object>();
                this.series = series;
                List<ValueTuple<int, int>> position = new List<(int, int)>();
                for (int i = 0; i < Math.Min(series.indexMap[slice.start].Count, series.indexMap[slice.end].Count); i++)
                {
                    position.Add((series.indexMap[slice.start][i], series.indexMap[slice.end][i]));
                }

                // generate index
                foreach (var pair in position)
                {
                    if (slice.step > 0)
                    {
                        for (int i = pair.Item1; i <= pair.Item2; i += slice.step)
                        {
                            if (removedDuplicatedIdx.Add(i))
                            {// If the conversion to integer index changes, add the corresponding index to the index list.
                                index.Add(series.index[i]);
                                this.convertedToIntIdx.Add(i);
                            }

                        }
                    }
                    if (slice.step < 0)
                    {
                        for (int i = pair.Item1; i >= pair.Item2; i += slice.step)
                        {
                            if (removedDuplicatedIdx.Add(i))
                            {// If the conversion to integer index changes, add the corresponding index to the index list.
                                index.Add(series.index[i]);
                                this.convertedToIntIdx.Add(i);
                            }
                        }
                    }

                }

            }
            public Series ToSeries()
            {
                List<object?> data = new List<object?>();
                foreach (var i in this.convertedToIntIdx)
                {
                    data.Add(series.values[i]);
                }
                var res = new Series(this.series.name, data, series.index);
                res.dtype = this.series.dtype;// bảo toàn kiểu dữ liệu gốc tránh bị suy luận kiểu làm đổi kiểu dữ liệu
                return res;
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
                this.convertedToIntIdx = NewConvertedToIntIdx.ToList();
                GC.Collect();
                return this;
            }
            public View GetView(List<object> subIndex) // this change the view of the current view
            {
                if (subIndex.Any(v => !this.index.Contains(v)))
                    throw new ArgumentOutOfRangeException("Sub-index contains values not in the current View");
                this.index = new List<object> { subIndex };
                List<int> ChangedIntIdx = new List<int>();
                foreach (object item in this.index)
                {
                    if (series.Contains(item))
                    {
                        ChangedIntIdx.AddRange(this.series.indexMap[item]);
                    }
                }
                this.convertedToIntIdx.Clear();
                this.convertedToIntIdx.AddRange(ChangedIntIdx);
                return this;
            }
            public IEnumerator<object?> GetValueEnumerator()
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
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Index | Value");
                sb.AppendLine("--------------");

                foreach (int IntIdx in this.convertedToIntIdx)
                {
                    sb.AppendLine($"{this.series.index[IntIdx],-6} | {this.series.values[IntIdx]?.ToString() ?? "null"}");
                }
                return sb.ToString();
            }
        }

        public class GroupView
        {
            private readonly Dictionary<object, int[]> groups; // this only store the index of the series values
            Series source;

            public GroupView(Series source, Dictionary<object, int[]> groupIndices)
            {
                this.source = source;
                this.groups = groupIndices;
            }
            // Lấy danh sách index của một nhóm
            private ReadOnlyMemory<int> GetGroupIndices(object key)
            {
                return groups.TryGetValue(key, out var indices) ? indices.AsMemory() : ReadOnlyMemory<int>.Empty;
            }
            public Dictionary<object, object> Sum()
            {
                var result = new Dictionary<object, object>();
                foreach (var key in this.groups.Keys)
                {
                    int[] indexes = this.groups[key];
                    dynamic? sum = Activator.CreateInstance(type: this.source.dtype);
                    foreach (var idx in indexes)
                    {
                        if (this.source.values[idx] != null && this.source.values[idx] != DBNull.Value)
                            sum += this.source.values[idx];
                    }
                    result.Add(key, sum);
                }
                return result;
            }
            public Dictionary<object, uint> Count()
            {
                var result = new Dictionary<object, uint>();
                foreach (var kvp in groups)
                {
                    result[kvp.Key] = (uint)kvp.Value.Length;
                }
                return result;
            }
        }

        // private method here
        private bool DeleteIndex(object indexKey)
        {
            if (!indexMap.ContainsKey(indexKey)) return false; // Nếu index không tồn tại, return false

            var positions = indexMap[indexKey];

            // Xóa index khỏi danh sách index
            index.Remove(indexKey);
            indexMap.Remove(indexKey);

            return true;
        }
        // private method end here

        // support method to check type is valid to add the data series
        public bool IsValidType(object? value)
        {
            return value == null
                 || value == DBNull.Value
                 || (value.GetType().IsValueType && dType.IsValueType && value.GetType() == dType)
                 || dType.IsAssignableFrom(value.GetType());
        }

        //constructor
        public Series(string? name, List<object?> values, List<object>? index = null)
        {
            this.name = name;
            this.values = new List<object?>(values ?? new List<object?>());

            // infer datatype
            this.dtype = Supporter.InferDataType(this.values);
            // handle index
            indexMap = new Dictionary<object, List<int>>();

            if (index == null)
            {
                this.defaultIndex = true;
                this.index = Enumerable.Range(0, values!.Count).Cast<object>().ToList();
                for (int i = 0; i < values.Count; i++)
                {
                    indexMap[i] = new List<int> { i };
                }
            }
            else
            {
                this.defaultIndex = false;
                this.index = new List<object>(index);
                if (index.Count != values!.Count)
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
            this.dtype = other.dtype;
            this.index = new List<object>(other.index);

            // Clone indexMap để tránh bị ảnh hưởng bởi object gốc
            this.indexMap = other.indexMap.ToDictionary(
                kvp => kvp.Key,
                kvp => new List<int>(kvp.Value) // Clone danh sách vị trí
            );
        }
        public Series(ValueTuple<string, List<object>> NameAndValues)
        {
            Supporter.CheckNull(NameAndValues);
            this.name = NameAndValues.Item1;
            this.values = new List<object?>(NameAndValues.Item2 ?? new List<object>());
            this.dtype = Supporter.InferDataType(this.values);
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
        public void Add(object? item, object? index = null)
        {
            if (!IsValidType(item))
            {
                try // trying cast item to proper data type to add
                {
                    if (dType == typeof(int) && int.TryParse(item?.ToString(), out int intValue))
                    {
                        this.values.Add(intValue);
                        return;
                    }
                    if (dType == typeof(double) && double.TryParse(item?.ToString(), out double DoubleValue))
                    {
                        this.values.Add(DoubleValue);
                        return;
                    }
                    if (dType == typeof(DateTime) && DateTime.TryParse(item?.ToString(), out DateTime DateTimeValue))
                    {
                        this.values.Add(DateTimeValue);
                        return;
                    }
                    var convertedItem = Convert.ChangeType(item, dType);
                    this.values.Add(convertedItem);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Expected type {dType}, but got {item?.GetType()}. You must change the đata type to {this.dtype} first");
                }
            }
            if (index == null)
            {
                if (!defaultIndex)
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
        public bool Remove(object? item, bool deleteIndexIfEmpty = true)
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

            if (!removed) return false;

            // Cleanup values và cập nhật lại indexMap
            var newValues = new List<object?>();
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
            this.values.Clear();
            this.index.Clear();
            this.indexMap.Clear();
        }
        public void UpdateValues(object index, List<object?> values)
        {
            // checking valid arguments
            if (!this.indexMap.TryGetValue(index, out var positions))
            {
                throw new InvalidOperationException($"index {nameof(index)} is not in series ");
            }
            if (values == null)
            {
                throw new ArgumentNullException("List of values must not be null", nameof(values));
            }
            if (values.Count == 0)
            {
                throw new InvalidOperationException("this action can't be done if values count is 0");
            }
            // check type validity
            var invalidValues = values.Where(v => !this.IsValidType(v)).ToList();
            if (invalidValues.Count > 0)
            {
                throw new ArgumentException(
                    $"The list contains {invalidValues.Count} invalid value(s): " +
                    $"{string.Join(", ", invalidValues.Select(v => v?.ToString() ?? "null"))}."
                );
            }
            if (values.Count > 1 && values.Count != positions.Count)
            {
                if (positions.Count == 0)
                {
                    throw new InvalidOperationException($"there are no elements can be replace at index{index.ToString()}.");
                }
                throw new ArgumentException($"Expected the length of the value to replace {positions.Count} or 1 but the actual length of value is {values.Count}");
            }
            // main logic of the method
            if (values.Count == 1)
            {
                foreach (var posítion in positions)
                {
                    this.values[posítion] = values[0];
                }
                return;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                this.values[i] = values[i];
            }
        }
        public void UpdateValues(Series other)
        {
            this.indexMap.Clear();
            this.values.Clear();
            this.index.Clear();
            for (int i = 0; i < other.values.Count; i++)
            {
                values[i] = other.values[i];
            }
            this.index = new List<object>(other.index);
            this.indexMap = new Dictionary<object, List<int>>();
            foreach (var key in other.indexMap.Keys)
            {
                this.indexMap[key] = new List<int>(other.indexMap[key]);
            }
        }

        // access the series
        public ISeries Head(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<object?> items = new List<object?>();
            for (int i = 0; i < count; i++)
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
            if (!indexMap.ContainsKey(slices.start))
            {
                throw new ArgumentException($"start index: {slices.start} not exist");
            }
            if (!indexMap.ContainsKey(slices.start))
            {
                throw new ArgumentException($"End index: {slices.end} not exist");
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

            var result = new Series(this.Name, newValues, this.index);
            result.dtype = NewType;
            return result;
        }
        public void Sort(Comparer<object?>? comparer = null)
        {
            if (values == null) { return; }
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
        // public View GetView(List<object> indexes){}
        public GroupView GroupByIndex()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            foreach (var Index in this.indexMap.Keys)
            {
                groups[Index] = this.indexMap[Index].ToArray();
            }
            return new GroupView(this, groups);
        }
        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> groups = new Dictionary<object, int[]>();
            var RemovedDuplicate = new HashSet<object?>(this.values);
            foreach (var Element in RemovedDuplicate)
            {
                int[] indicies = this.values.Select((value, index) => new { value, index })
                                        .Where(x => Object.Equals(x, Element))
                                        .Select(x => x.index)
                                        .ToArray();
                if (Element == null)
                {
                    object temp = DBNull.Value;
                    groups[temp] = indicies;
                }
                else
                {
                    groups[Element] = indicies;
                }

            }
            return new GroupView (this, groups);    

        }

        // searching and filter
        public IList<object?> Filter(Func<object?, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }

        public List<int> Find(object? item) // find all occurance of item
        {
            List<int> Indexes = new List<int>();
            for (int i = 0; i < this.Count; i++)
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

        public void resetIndex()
        {
            this.index = Enumerable.Range(0, values.Count - 1).Cast<object>().ToList();
            this.defaultIndex = true;
            this.indexMap.Clear();
            foreach (var idx in index)
            {
                this.indexMap[idx] = new List<int> { Convert.ToInt32(idx) };
            }
        }
        // methods end here
    }
}
