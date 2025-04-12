using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType> : ISeries<DataType> where DataType : notnull
    {
        private string? name;
        private List<DataType> values;
        private Dictionary<object, List<int>> indexMap;
        private List<object> index;
        private bool defaultIndex = false;

        //handle multi thread
        private ReaderWriterLock rwl = new ReaderWriterLock();

        // inner class
        public class View
        {
            private Series<DataType> series;
            private List<object> index;
            private List<int> intIndexList = new List<int>();

            public View(Series<DataType> series, List<object> index)
            {
                ArgumentNullException.ThrowIfNull(series);
                ArgumentNullException.ThrowIfNull(index);

                this.series = series;
                this.index = new List<object>();

                foreach (var idx in index)
                {
                    if (!series.indexMap.TryGetValue(index, out var positions))
                    {
                        throw new IndexOutOfRangeException($"Index {index} out of range");
                    }

                    this.index.Add(index);
                    this.intIndexList.AddRange(positions); // Tránh lặp nhiều lần
                }
            }


            public View(Series<DataType> series, (object start, object end, int step) slice)
            {
                Supporter.OrderedSet<int> removedDuplicateIdx = new Supporter.OrderedSet<int>();
                // check valid argument
                ArgumentNullException.ThrowIfNull(series);
                if (slice.step == 0) throw new ArgumentException("step must not be 0");
                if (!series.indexMap.TryGetValue(slice.start, out var startList))
                    throw new ArgumentException("start is not exist");
                if (!series.indexMap.TryGetValue(slice.end, out var endList))
                    throw new ArgumentException("end is not exist");

                // main logic of the method
                this.index = new List<object>();
                this.series = series;

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
                            if (removedDuplicateIdx.Add(i))
                            {
                                index.Add(series.index[i]);
                                this.intIndexList.Add(i);
                            }
                        }
                    }
                    if (slice.step < 0)
                    {
                        for (int i = pair.Item1; i >= pair.Item2; i += slice.step)
                        {
                            if (removedDuplicateIdx.Add(i))
                            {
                                index.Add(series.index[i]);
                                this.intIndexList.Add(i);
                            }
                        }
                    }
                }
            }

            public View GetView(List<object> subIndex) // change the view
            {
                if (subIndex.Any(v => !this.index.Contains(v)))
                {
                    throw new ArgumentOutOfRangeException(nameof(subIndex),"Sub index contains values not in the current View");
                }
              
                this.index = new List<object> { subIndex };
                return this;
            }
            public Series<DataType> ToSeries(string? name = null)
            {
                List<DataType>values = new List<DataType>();
                foreach (var idx in this.intIndexList)
                {
                    values.Add(this.series.values[idx]);
                }
                return new Series<DataType>(name, values, this.index);
            }

            public void UpdateValue(object indexx, DataType newValue)
            {
                if (!index.Contains(index))
                    throw new KeyNotFoundException($"Index:{index} not in View");

                // Chỉ cập nhật những phần tử thuộc View
                this.series = ToSeries(this.series.name);
                foreach (var pos in series.indexMap[index])
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
                Supporter.OrderedSet<int> NewIntIndexList = new Supporter.OrderedSet<int>();

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
                            if (NewIntIndexList.Add(j))
                                newIndex.Add(this.index[j]);
                        }
                    }
                    else
                    {
                        for (int j = startPos; j >= endPos; j += slice.step)
                        {
                            if (NewIntIndexList.Add(j))
                                newIndex.Add(this.index[j]);
                        }
                    }
                }

                this.index = newIndex;
                this.intIndexList = NewIntIndexList.ToList();
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
                            if (this.intIndexList.Contains(pos))
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
                foreach (var idx in this.index)
                {
                    foreach (var val in this.series[idx])
                    {
                        sb.AppendLine($"{idx,-6} | {val?.ToString() ?? "null"}");
                    }
                }
                return sb.ToString();
            }
        }
        public class GroupView
        {
            private readonly Dictionary<object, int[]> groups; // this only store the index of the series values
            Series<DataType> source;

            public GroupView(Series<DataType> source, Dictionary<object, int[]> groupIndices)
            {
                this.source = source;
                this.groups = groupIndices;
            }

            // Lấy danh sách index của một nhóm
            private ReadOnlyMemory<int> GetGroupIndices(object key)
            {
                return groups.TryGetValue(key, out var indices) ? indices.AsMemory() : ReadOnlyMemory<int>.Empty;
            }

            public Series<DataType> GetGroup(object key, string? newName = "")
            {
                if (!groups.TryGetValue(key, out var indices))
                    throw new KeyNotFoundException($"Nhóm {key} không tồn tại.");
                var values = new List<DataType>(indices.Length);
                var indexes = new List<object>(indices.Length);
                foreach (var idx in indices)
                {
                    values.Add(this.source.values[idx]);
                    indexes.Add(this.source.index[idx]);
                }
                return new Series<DataType>(newName, values, indexes);
            }
            public Dictionary<object, DataType> Sum()
            {
                var result = new Dictionary<object, DataType>();

                foreach (var kvp in groups)
                {
                    object key = kvp.Key;
                    int[] indices = kvp.Value;

                    dynamic? sum = default(DataType);
                    foreach (var idx in indices)
                    {
                        if (this.source.values[idx] != null)
                        {
                            sum += (dynamic)this.source.values[idx];
                        }

                        if (this.source.values[idx] is object value && value != DBNull.Value)
                        {
                            sum += (dynamic)value;
                        }
                    }

                    result[key] = sum;
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
        public Series()
        {
            this.name = "";
            this.indexMap = new Dictionary<object, List<int>>();
            this.values = new List<DataType>();
            this.index = new List<object> { };
        }
        public Series(string? name, List<DataType> values, List<object>? index = null)
        {
            this.name = name;
            if (values == null)
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
        public Series(Tuple<string?, List<DataType>> KeyAndValues)
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
    }
}
