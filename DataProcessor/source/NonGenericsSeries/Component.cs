using System.Collections;
using System.Text;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series : ISeries
    {
        private string? seriesName;
        private List<object?> values;
        private Dictionary<object, List<int>> indexMap;
        private List<object> index;
        private IndexSynchronizer synchronizer = new IndexSynchronizer();
        private Type dataType;

        // handle multi threads, this will be implemented in the future
        private readonly Semaphore writeSemaphore = new Semaphore(1, 1);
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        // inner class
        public class View : IndexChangeListener, IDisposable
        {
            private List<object> index;
            private Series series;
            private List<int> convertedToIntIdx = new List<int>();
            private bool shouldSyncIndex = true;

            // properties
            public IReadOnlyList<object> Index => index;
            public bool IsView { get; private set; }
            public int Count => convertedToIntIdx.Count;

            // methods
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
                        throw new ArgumentException($"{index} is not in the index of the series");
                    }

                    this.index.Add(index);
                    this.convertedToIntIdx.AddRange(positions); // Tránh lặp nhiều lần
                }
                series.synchronizer.RegisterView(this);
            }

            public View(Series series, (object start, object end, int step) slice)
            {
                //check valid argument
                ArgumentNullException.ThrowIfNull(series);
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                if (!series.index.Contains(slice.start)) { throw new ArgumentException($"start index {slice.start} does not exist"); }
                if (!series.index.Contains(slice.end)) { throw new ArgumentException($"end index {slice.end} does not exist"); }

                // main method logic
                Supporter.OrderedSet<int> removedDuplicatedIndex = new Supporter.OrderedSet<int>();
                this.index = new List<object>();
                this.series = series;
                List<ValueTuple<int, int>> pairPosition = new List<(int, int)>();
                for (int i = 0; i < Math.Min(series.indexMap[slice.start].Count, series.indexMap[slice.end].Count); i++)
                {
                    pairPosition.Add((series.indexMap[slice.start][i], series.indexMap[slice.end][i]));
                }

                // generate index
                foreach (var pair in pairPosition)
                {
                    if (slice.step > 0)
                    {
                        for (int i = pair.Item1; i <= pair.Item2; i += slice.step)
                        {
                            if (removedDuplicatedIndex.Add(i))
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
                            if (removedDuplicatedIndex.Add(i))
                            {// If the conversion to integer index changes, add the corresponding index to the index list.
                                index.Add(series.index[i]);
                                this.convertedToIntIdx.Add(i);
                            }
                        }
                    }

                }
                series.synchronizer.RegisterView(this);
            }

            public Series ToSeries(string? seriesName = null)// Tạo một Series mới từ các giá trị trong view, giữ nguyên index và datatype gốc

            {
                List<object?> data = new List<object?>();
                foreach (var i in this.convertedToIntIdx)
                {
                    data.Add(series.values[i]);
                }

                var result = new Series(data, seriesName, this.index)
                {
                    dataType = series.dataType// bảo toàn kiểu dữ liệu gốc tránh bị suy luận kiểu làm đổi kiểu dữ liệu
                };

                return result;
            }

            public void UpdateValue(object index,List<object?> newValues, bool inPlace = false)
            {
                if (!this.index.Contains(index))
                {
                    throw new IndexOutOfRangeException($"Index {index} not in view");
                }
                if (newValues.Count == 0)
                {
                    return;
                }
                List<int> positions = series.indexMap[index];
                if (newValues.Count != positions.Count && newValues.Count != 1)
                {
                    throw new InvalidOperationException($"index has {positions.Count} value but there are {newValues.Count} items ready for replace");
                }
                
                shouldSyncIndex = false;
                if (!inPlace)
                {
                    IsView = false;
                    this.series = this.ToSeries(null);
                    if (newValues.Count == 1)
                    {
                        foreach (var position in positions)
                        {
                            series.values[position] = newValues[0];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < positions.Count; i++)
                        {
                            series.values[i] = newValues[i];
                        }
                    }
                }
                else
                {
                    IsView = true;
                    if (newValues.Count == 1)
                    {
                        foreach (var position in positions)
                        {
                            series.values[position] = newValues[0];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < positions.Count; i++)
                        {
                            series.values[i] = newValues[i];
                        }
                    }
                }
                    
            }

            public View GetView((object start, object end, int step) slice) // this just change view of the current vỉew
            {
                // synchroization with the origin series
                for (int i = 0; i < this.convertedToIntIdx.Count; i++)
                {
                    foreach (var key in series.indexMap.Keys)
                    {
                        if (!series.indexMap[key].Contains(this.convertedToIntIdx[i]))
                        {
                            this.index[i] = key;
                        }
                    }
                }

                // check valid argument
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
                // synchroization with the origin series
                if (this.index != series.index)
                {
                    this.index = new List<object> { series.index };
                }

                // main logic method
                if (subIndex.Any(v => !this.index.Contains(v)))
                    throw new ArgumentOutOfRangeException(nameof(subIndex), "Sub-index contains values not in the current View");
                this.index = new List<object> { subIndex };
                List<int> ChangedIntIdx = new List<int>();
                foreach (var item in subIndex)
                {
                    var positions = series.indexMap[item];
                    foreach (var position in positions)
                    {
                        if (this.convertedToIntIdx.Contains(position))
                        {
                            ChangedIntIdx.Add(position);
                        }
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
                    if (series.indexMap.TryGetValue(idx, out var positions))
                    {
                        foreach (var pos in positions)
                        {
                            if (this.convertedToIntIdx.Contains(pos))
                                yield return series.Values[pos];
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
                    sb.AppendLine($"{series.index[IntIdx],-6} | {series.values[IntIdx]?.ToString() ?? "null"}");
                }
                return sb.ToString();
            }

            public void UpdateIndex()
            {
                if (shouldSyncIndex)
                {
                    index.Clear();
                    foreach (var IntIndex in this.convertedToIntIdx)
                    {
                        foreach (var seriesIndex in series.indexMap.Keys)
                        {
                            if (series.indexMap[seriesIndex].Contains(IntIndex))
                            {
                                index.Add(seriesIndex);
                            }
                        }
                    }
                }              
            }

            void IDisposable.Dispose()
            {
                
            }
        }

        public class GroupView(Series source, Dictionary<object, int[]> groupIndices)
        {
            private readonly Dictionary<object, int[]> groups = groupIndices; // this only store the index of the series values
            Series source = source;

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
                    dynamic? sum = Activator.CreateInstance(type: this.source.dataType);
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

        //constructor
        public Series(List<object?> values, string? seriesName = null, List<object>? index = null)
        {
            this.seriesName = seriesName;
            this.values = new List<object?>(values);
            this.indexMap = new Dictionary<object, List<int>>();

            // Handle index and map
            if (index != null)
            {
                this.index = new List<object>(index);
                for (int i = 0; i < index.Count; i++)
                {
                    // Add index to map (if not exists, create new entry)
                    if (!indexMap.TryGetValue(index[i], out var list))
                    {
                        list = new List<int>();
                        indexMap[index[i]] = list;
                    }
                    list.Add(i);
                }

                // Ensure values list is extended to match the index size
                while (values.Count < index.Count)
                {
                    values.Add(null);
                }
            }
            else
            {
                this.index = new List<object>(Enumerable.Range(0, values.Count).Cast<object>());
                for (int i = 0; i < values.Count; i++)
                {
                    indexMap[i] = new List<int> { i };
                }
            }

            this.dataType = Support.InferDataType(this.values);
        }
        public Series(Series other)
        {
            Supporter.CheckNull(other);
            this.seriesName = other.seriesName;
            this.values = new List<object?>(other.values);
            this.dataType = other.dataType;
            this.index = new List<object>(other.index);

            // Clone indexMap để tránh bị ảnh hưởng bởi object gốc
            this.indexMap = other.indexMap.ToDictionary(
                kvp => kvp.Key,
                kvp => new List<int>(kvp.Value) // Clone danh sách vị trí
            );
        }
        public Series(ValueTuple<string, List<object>> NameAndValues)
        {
            this.seriesName = NameAndValues.Item1;
            this.values = new List<object?>(NameAndValues.Item2 ?? new List<object>());
            this.dataType = Support.InferDataType(this.values);
            indexMap = new Dictionary<object, List<int>>();
            index = Enumerable.Range(0, values.Count - 1).Cast<object>().ToList();
            for (int i = 0; i < values.Count; i++)
            {
                indexMap[i] = new List<int> { i };
            }
        }
        public Series()
        {
            this.seriesName = "";
            this.values = new List<object?>();
            this.index = new List<object>();
            this.indexMap = new Dictionary<object, List<int>>();
            this.dataType = typeof(object);
        }

        // iterator
        public IEnumerator<object?> GetEnumerator()
        {
            return this.values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
