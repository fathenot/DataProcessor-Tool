using DataProcessor.source.Index;
using DataProcessor.source.ValueStorage;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
namespace DataProcessor.source.NonGenericsSeries
{

    // this partial class contains the components of the series class, which also includes inner classes like SeriesVIew and GroupView
    public partial class Series : ISeries
    {

        private string? seriesName;
        private IIndex index;
        private AbstractValueStorage valueStorage;
        public Type dataType; // data type of the series, can be null if empty or not set

        // handle multi threads, this will be implemented in the future
        private readonly Semaphore writeSemaphore = new Semaphore(1, 1);
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Represents a view over a <see cref="Series"/>, allowing slicing and filtering without copying all data.
        /// </summary>
        public class SeriesView : IEnumerable<object?>
        {
            private readonly Series _series;
            private readonly List<int> _indices;       // Positions in the original series
            private readonly List<object> _viewIndices; // Corresponding index values
            private readonly HashSet<object?> _viewIndexSet; // For fast index existence lookup

            #region Constructors

            /// <summary>
            /// Internal constructor used to create a view with pre-computed indices.
            /// </summary>
            internal SeriesView(Series series, List<int> indices, List<object> viewIndices)
            {
                _series = series ?? throw new ArgumentNullException(nameof(series));
                _indices = indices ?? throw new ArgumentNullException(nameof(indices));
                _viewIndices = viewIndices ?? throw new ArgumentNullException(nameof(viewIndices));
                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            /// <summary>
            /// Initializes a view from a range specification.
            /// </summary>
            /// <param name="series">The source series.</param>
            /// <param name="slices">A tuple containing start index, end index, and step.</param>
            /// <exception cref="ArgumentException">Thrown if start or end index does not exist, or step is zero.</exception>
            public SeriesView(Series series, (object start, object end, int step) slices)
            {
                this._series = series ?? throw new ArgumentNullException( nameof(series));
                var tmp = BuildIndicesFromRange(series, slices.start, slices.end, slices.step);
                _viewIndices = tmp.viewIndices;
                _indices = tmp.indices;
                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            /// <summary>
            /// Initializes a view from a list of index values.
            /// </summary>
            /// <param name="series">The source series.</param>
            /// <param name="slice">List of index values to include in the view.</param>
            /// <exception cref="ArgumentException">Thrown if any index in slice does not exist in the series.</exception>
            public SeriesView(Series series, List<object> slice)
            {
                _series = series;
                var tmp = BuildIndicesFromList(series,slice);
                _viewIndices = tmp.viewIndices;
                _indices = tmp.indices;
                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            #endregion

            #region Private Static Builders

            private static (List<int> indices, List<object> viewIndices) BuildIndicesFromRange(Series series, object start, object end, int step)
            {
                if (!series.index.Contains(start) || !series.index.Contains(end))
                    throw new ArgumentException("Start or end index does not exist in the series index.");
                if (step == 0)
                    throw new ArgumentException("Step cannot be zero.");

                var indices = new List<int>();
                var viewIndices = new List<object>();

                int startIdx = series.index.FirstPositionOf(start);
                int endIdx = series.index.FirstPositionOf(end);

                if (step > 0)
                {
                    for (int i = startIdx; i <= endIdx; i += step)
                    {
                        indices.Add(i);
                        viewIndices.Add(series.index.GetIndex(i));
                    }
                }
                else
                {
                    for (int i = startIdx; i >= endIdx; i += step)
                    {
                        indices.Add(i);
                        viewIndices.Add(series.index.GetIndex(i));
                    }
                }

                return (indices, viewIndices);
            }

            private static (List<int> indices, List<object> viewIndices) BuildIndicesFromList(Series series, List<object> slice)
            {
                var indices = new List<int>();
                var viewIndices = new List<object>();

                foreach (var item in slice)
                {
                    if (!series.index.Contains(item))
                        throw new ArgumentException($"Index {item} does not exist in the series index.");

                    foreach (var pos in series.index.GetIndexPosition(item))
                    {
                        indices.Add(pos);
                        viewIndices.Add(series.index.GetIndex(pos));
                    }
                }

                return (indices, viewIndices);
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Creates a new view from the current view, including only the specified index values.
            /// </summary>
            public SeriesView SliceView(List<object> slice)
            {
                var requested = new HashSet<object>(slice);
                var newIndices = new List<int>();
                var newViewIndices = new List<object>();

                for (int i = 0; i < _indices.Count; i++)
                {
                    var idx = _viewIndices[i];
                    if (requested.Contains(idx))
                    {
                        newIndices.Add(_indices[i]);
                        newViewIndices.Add(idx);
                    }
                }

                return new SeriesView(_series, newIndices, newViewIndices);
            }

            /// <summary>
            /// Creates a new view by slicing with start/end positions and step.
            /// </summary>
            public SeriesView SliceView((int start, int end, int step) slices)
            {
                if (slices.step == 0)
                    throw new ArgumentException("Step cannot be zero.");
                if (slices.start < 0 || slices.start >= _viewIndices.Count ||
                    slices.end < 0 || slices.end >= _viewIndices.Count)
                    throw new ArgumentOutOfRangeException("Start or end position is out of range.");

                var newIndices = new List<int>();
                var newViewIndices = new List<object>();

                if (slices.step > 0)
                {
                    for (int i = slices.start; i <= slices.end; i += slices.step)
                    {
                        newIndices.Add(_indices[i]);
                        newViewIndices.Add(_viewIndices[i]);
                    }
                }
                else
                {
                    for (int i = slices.start; i >= slices.end; i += slices.step)
                    {
                        newIndices.Add(_indices[i]);
                        newViewIndices.Add(_viewIndices[i]);
                    }
                }

                return new SeriesView(_series, newIndices, newViewIndices);
            }

            /// <summary>
            /// Converts the current view to a new <see cref="Series"/> instance.
            /// </summary>
            public Series ToSeries(string? name = null)
            {
                var values = _indices.Select(pos => _series.valueStorage.GetValue(pos)).ToList();
                return new Series(values, index: _viewIndices, dtype: _series.dataType, name: name ?? _series.seriesName, copy: true);
            }

            #endregion

            #region Indexers & Enumerators

            /// <summary>
            /// Gets all values associated with a given index in the current view.
            /// </summary>
            public IEnumerable<object?> this[object index]
            {
                get
                {
                    if (!_viewIndexSet.Contains(index))
                        throw new ArgumentException($"Index {index} does not exist in the view.");

                    foreach (var pos in _series.index.GetIndexPosition(index))
                    {
                        if (_indices.Contains(pos))
                            yield return _series.valueStorage.GetValue(pos);
                    }
                }
            }

            public IEnumerator<object?> GetEnumerator()
            {
                foreach (var pos in _indices)
                    yield return _series.valueStorage.GetValue(pos);
            }

            IEnumerator<object?> IEnumerable<object?>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion

            #region Overrides & Properties

            /// <summary>
            /// Returns a formatted string representation of the current view.
            /// </summary>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Series View: {_series.seriesName ?? "Unnamed"}");
                sb.AppendLine("Index | Value");
                sb.AppendLine("--------------");

                for (int i = 0; i < _indices.Count; i++)
                    sb.AppendLine($"{_viewIndices[i],5} | {_series.valueStorage.GetValue(_indices[i])?.ToString() ?? "null"}");

                return sb.ToString();
            }

            /// <summary>Gets the number of elements in the view.</summary>
            public int Count => _indices.Count;

            /// <summary>Gets a read-only list of positions in the original series.</summary>
            public IReadOnlyList<int> Indices => _indices;

            /// <summary>Gets a read-only list of index values in the view.</summary>
            public IReadOnlyList<object> ViewIndices => _viewIndices;

            #endregion
        }


        /// <summary>
        /// Represents a view of grouped data, providing functionality to retrieve, summarize, and count values based on
        /// group keys.
        /// </summary>
        /// <remarks>The <see cref="GroupView"/> class is designed to work with a series of data and a
        /// grouping structure that maps keys to indices. It provides methods to calculate summaries, count elements,
        /// and access grouped values. This class is useful for scenarios where data needs to be analyzed or aggregated
        /// based on predefined groupings.</remarks>
        public class GroupView
        {
            private Series _series;
            private Dictionary<object, List<int>> _groups;
            public GroupView(Series series, Dictionary<object, List<int>> groups)
            {
                _series = series;
                _groups = groups;
            }

            /// <summary>
            /// Retrieves the indices associated with the specified key.
            /// </summary>
            /// <param name="key">The key used to look up the group indices. Must not be <see langword="null"/>.</param>
            /// <returns>A read-only memory segment containing the indices associated with the specified key. If the key is not
            /// found, returns an empty <see cref="ReadOnlyMemory{T}"/>.</returns>
            internal ReadOnlyMemory<int> GetGroupIndices(object key)
            {
                return _groups.TryGetValue(key, out var indices) ? indices.ToArray().AsMemory() : ReadOnlyMemory<int>.Empty;
            }

            /// <summary>
            /// Calculates the sum of values for each group and returns the results as a dictionary.
            /// </summary>
            /// <remarks>This method iterates through all groups and computes the sum of values
            /// associated with each group. The resulting dictionary contains the group keys as keys and the computed
            /// sums as values. Null indices in the series are excluded from the summation. It can handle custom datatype if the datatype supports
            /// the '+' operator</remarks>
            /// <returns>A dictionary where each key represents a group and the corresponding value is the sum of values for that
            /// group. The value is dynamically typed based on the data type of the series.</returns>
            public Dictionary<object, object> Sum()
            {
                var result = new Dictionary<object, object>();
                foreach (var key in this._groups.Keys)
                {
                    List<int> indexes = this._groups[key];
                    dynamic? sum = Activator.CreateInstance(type: this._series.dataType);
                    var nullIndices = this._series.valueStorage.NullIndices.ToHashSet(); // get null indices from the series
                    var converter = TypeDescriptor.GetConverter(this._series.dataType);
                    foreach (var idx in indexes)
                    {
                        if (!nullIndices.Contains(idx)) // performance of this check is not optimal, but it is necessary to avoid null values in the sum
                        {
                            object? val = this._series.valueStorage.GetValue(idx);

                            dynamic convertedVal = converter.ConvertFrom(val!)!;
                            sum += convertedVal;

                        }
                    }
                    result.Add(key, sum);
                }
                return result;
            }

            /// <summary>
            /// Counts the number of elements in each group and returns the results as a dictionary.
            /// </summary>
            /// <returns>A dictionary where each key represents a group and the corresponding value is the count of elements in
            /// that group.</returns>
            public Dictionary<object, uint> Count()
            {
                var result = new Dictionary<object, uint>();
                foreach (var kvp in _groups)
                {
                    result[kvp.Key] = (uint)kvp.Value.Count;
                }
                return result;
            }


            /// <summary>
            /// Gets the collection of keys used to group items.
            /// </summary>
            public IEnumerable<object> GroupKeys => _groups.Keys;

            /// <summary>
            /// Gets the collection of values associated with the specified key.
            /// </summary>
            /// <remarks>This indexer retrieves all values corresponding to the given key from the
            /// underlying data structure. The returned collection may contain null values if the data source includes
            /// null entries.</remarks>
            /// <param name="key">The key identifying the group of values to retrieve.</param>
            /// <returns>An enumerable collection of values associated with the specified key. If the key does not exist, a <see
            /// cref="KeyNotFoundException"/> is thrown.</returns>
            /// <exception cref="KeyNotFoundException">Thrown if the specified <paramref name="key"/> does not exist in the collection.</exception>
            public IEnumerable<object?> this[object key]
            {
                get
                {
                    if (!_groups.ContainsKey(key))
                        throw new KeyNotFoundException($"Group key '{key}' not found.");
                    foreach (var index in _groups[key])
                    {
                        yield return _series.valueStorage.GetValue(index);
                    }
                }
            }
        }


        // this part is iteator, which allows the series to be enumerated

        /// <summary>
        /// Returns an enumerator that iterates through the collection of values.
        /// </summary>
        /// <returns>An enumerator for the collection of values stored in this instance.</returns>
        public IEnumerator<object?> GetEnumerator()
        {
            return this.valueStorage.GetEnumerator();
        }

        public IEnumerable<T?> AsTyped<T>()
        {
            if (typeof(T).IsAssignableFrom(dataType))
            {
                return valueStorage.AsTyped<T>();
            }
            return valueStorage.AsTyped<T>();
        }
    }
}