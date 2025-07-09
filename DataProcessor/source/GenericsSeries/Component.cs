using DataProcessor.source.Index;
using DataProcessor.source.UserSettings.DefaultValsGenerator;
using DataProcessor.source.ValueStorage;
using System.Collections;
using System.Text;

namespace DataProcessor.source.GenericsSeries
{
    /// <summary>
    /// Represents a collection of data values indexed by a customizable index type.
    /// </summary>
    /// <remarks>The <see cref="Series{DataType}"/> class provides functionality for managing and manipulating
    /// a collection of data values indexed by a customizable index type. It supports operations such as filtering,
    /// slicing, grouping, and aggregation. The series is thread-safe for read and write operations using a <see
    /// cref="ReaderWriterLock"/> mechanism.  This class is designed to handle scenarios where data is associated with
    /// non-numeric or complex indices, such as time-series data or categorical data. It allows for flexible indexing
    /// and provides views for subsets of data, as well as group-based operations.</remarks>
    /// <typeparam name="DataType">The type of data stored in the series. Must be a non-nullable type.</typeparam>
    public partial class Series<DataType> : ISeries<DataType> where DataType : notnull
    {
        private string? name;
        private GenericsStorage<DataType> values; // this is the storage of the series values
        private IIndex index; // this is the index of the series, it can be any type of index

        //handle multi thread
        private ReaderWriterLock rwl = new ReaderWriterLock();

        // inner class

        /// <summary>
        /// Represents a view of a subset of data within a <see cref="Series{DataType}"/> object.
        /// </summary>
        /// <remarks>A <see cref="SeriesView"/> provides a filtered or sliced view of the data in a <see
        /// cref="Series{DataType}"/>. It allows operations such as creating subviews based on specific indices or
        /// slices, and converting the view back into a new <see cref="Series{DataType}"/> object. The view maintains a
        /// reference to the original series and ensures that all indices in the view exist within the original
        /// series.</remarks>
        public class SeriesView
        {
            private Series<DataType> series;
            private List<object> index;
            private List<int> intIndexList = new List<int>();
            private HashSet<object> indexSet = new HashSet<object>(); // this is used to check if the index is exist in the view

            private SeriesView(Series<DataType> series, List<object> index, List<int> viewIndices)
            {
                this.series = series ?? throw new ArgumentNullException(nameof(series));
                if (index == null || index.Count == 0)
                {
                    throw new ArgumentException("Index cannot be null or empty", nameof(index));
                }
                this.index = new List<object>(index);
                this.intIndexList = new List<int>(viewIndices);
                this.indexSet = new HashSet<object>(index); // create a set for fast lookup
            }
            public SeriesView(Series<DataType> series, List<object> index)
            {
                this.series = series ?? throw new ArgumentNullException(nameof(series));
                if (index == null || index.Count == 0)
                {
                    throw new ArgumentException("Index cannot be null or empty", nameof(index));
                }
                this.index = new List<object>(index.Count);
                foreach (var idx in index)
                {
                    this.index.Add(idx);
                    if (!series.index.Contains(idx))
                    {
                        throw new ArgumentException($"Index {idx} does not exist in the series", nameof(index));
                    }
                    foreach (int i in series.index.GetIndexPosition(idx))
                    {
                        this.intIndexList.Add(i);
                    }
                }

                this.indexSet = new HashSet<object>(this.index); // create a set for fast lookup
            }


            public SeriesView(Series<DataType> series, (object start, object end, int step) slice)
            {
                if (series == null) throw new ArgumentNullException(nameof(series));
                if (slice.step == 0) throw new ArgumentException("step must not be 0", nameof(slice.step));
                if (!series.index.Contains(slice.start) || !series.index.Contains(slice.end)) { throw new ArgumentException("start or end is not exist"); }

                this.series = series;


                int startIndex = series.index.FirstPositionOf(slice.start);
                int endIndex = series.index.FirstPositionOf(slice.end);

                this.index = new List<object>((endIndex - startIndex) / slice.step + 1);
                this.intIndexList = new List<int>();
                this.indexSet = new HashSet<object>(); // create a set for fast lookup

                if (slice.step > 0)
                {
                    for (int i = startIndex; i <= endIndex; i += slice.step)
                    {
                        this.index.Add(series.index[i]);
                        this.intIndexList.Add(i);
                        this.indexSet.Add(series.index[i]);
                    }
                }
                else
                {
                    for (int i = startIndex; i >= endIndex; i += slice.step)
                    {
                        this.index.Add(series.index[i]);
                        this.intIndexList.Add(i);
                        this.indexSet.Add(series.index[i]);
                    }
                }
            }

            /// <summary>
            /// Creates a new <see cref="SeriesView"/> based on the specified subset of indices.
            /// </summary>
            /// <remarks>This method filters the current view to include only the specified indices.
            /// The resulting view will contain the subset of data corresponding to the provided indices.</remarks>
            /// <param name="slice">A list of indices to include in the new view. Each index must exist in the current view.</param>
            /// <returns>A new <see cref="SeriesView"/> containing the specified indices.</returns>
            /// <exception cref="ArgumentException">Thrown if any index in <paramref name="slice"/> does not exist in the current view.</exception>
            public SeriesView GetView(List<object> slice) // change the view
            {
                List<object> newIndex = new List<object>(slice.Count);
                List<int> newIntIndexList = new List<int>();

                foreach (var idx in slice)
                {
                    if (!this.indexSet.Contains(idx)) throw new ArgumentException($"Index {idx} does not exist in the view", nameof(slice));
                    newIndex.Add(idx);
                    foreach (int i in this.series.index.GetIndexPosition(idx))
                    {
                        if (this.intIndexList.Contains(i)) newIntIndexList.Add(i);
                    }
                }
                return new SeriesView(this.series, newIndex, newIntIndexList);
            }
            public Series<DataType> ToSeries(string? name = null)
            {
                return new Series<DataType>(
                    this.intIndexList.Select(idx => (DataType)this.series.values.GetValue(idx)).ToList(),
                    name,
                    this.index.ToList()
                );
            }

            public SeriesView GetView((object start, object end, int step) slice) // this just change view of the current vỉew
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
                return this.intIndexList.Select(idx => this.series.values.GetValue(idx)).Cast<DataType>()
                   .GetEnumerator();
            }

            public IEnumerator<object> GetIndexEnumerator()
            {
                return this.index.GetEnumerator();
            }

            public override string ToString()
            {
                var stngBuilder = new StringBuilder();
                stngBuilder.AppendLine($"SeriesView of {series.Name ?? "Unnamed Series"}");
                stngBuilder.AppendLine("Index | Value");
                stngBuilder.AppendLine("--------------");
                for (int i = 0; i < this.intIndexList.Count; i++)
                {
                    stngBuilder.AppendLine($"{this.index[i],5} | {this.series.values.GetValue(this.intIndexList[i])?.ToString() ?? "null"}");
                }
                return stngBuilder.ToString();
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
                    values.Add((DataType)this.source.values.GetValue(idx));
                    indexes.Add(this.source.index[idx]);
                }
                return new Series<DataType>(values, newName, indexes);
            }
            public Dictionary<object, DataType> Sum(ICalculator<DataType>? aggregator, IDefaultValueGenerator<DataType>? defaultValueGenerator = null)
            {
                // check valid arguments
                if (aggregator == null)
                {
                    throw new ArgumentException("Aggregator cannot be null. Please provide a valid aggregator.");
                }
                var result = new Dictionary<object, DataType>();

                foreach (var kvp in groups)
                {
                    object key = kvp.Key;
                    int[] indices = kvp.Value;
                    DataType sụm = defaultValueGenerator != null ? defaultValueGenerator.GenerateDefaultValue() : default(DataType);

                    if (aggregator != null)
                    {
                        foreach (var idx in indices)
                        {
                            if (this.source.values[idx] != null)
                            {
                                sụm = aggregator.Add(sụm, (DataType)this.source.values.GetValue(idx));
                            }
                        }
                        result[key] = sụm;
                    }
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

        public sealed class GenericsSeriesEnumerator<DataType> : IEnumerator<DataType>
        {
            private readonly Series<DataType> series;
            private int _currentIndex = -1;

            public GenericsSeriesEnumerator(Series<DataType> series)
            {
                this.series = series;
            }

            public DataType Current => (DataType)series.values.GetValue(_currentIndex);

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < series.Count;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose()
            { 
                //no op
            }
        }

        internal record struct IndexedValue(DataType Value, object Index);
    }
}
