using DataProcessor.source.Index;
using DataProcessor.source.ValueStorage;
using System.Collections;
using System.ComponentModel;
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
        /// Represents a sliced view of a <see cref="Series"/> object, allowing access to a subset of its
        /// data.
        /// </summary>
        /// <remarks>A <see cref="SeriesView"/> provides a way to work with a subset of the data in a <see
        /// cref="Series"/>. It supports slicing by index ranges, specific index values, or step intervals. The view
        /// maintains a mapping between the original series indices and the indices in the view, enabling efficient
        /// lookups.  This class is enumerable, allowing iteration over the values in the view. It also provides methods
        /// for creating new views based on slices or converting the view back into a <see cref="Series"/>.</remarks>
        public class SeriesView : IEnumerable<object?>
        {
            private readonly Series _series;
            private readonly List<int> _indices; // positions in the original series
            private readonly List<object> _viewIndices; // corresponding indices
            private readonly HashSet<object?> _viewIndexSet; // for fast lookup

            // Primary constructor for internal use
            internal SeriesView(Series series, List<int> indices, List<object> viewIndices)
            {
                _series = series;
                _indices = indices;
                _viewIndices = viewIndices;
                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            // Constructor using (start, end, step)
            public SeriesView(Series series, (object start, object end, int step) slices)
            {
                if (!series.index.Contains(slices.start) || !series.index.Contains(slices.end))
                    throw new ArgumentException("Start or end index does not exist in the series index.");
                if (slices.step == 0)
                    throw new ArgumentException("Step cannot be zero.");

                _series = series;
                _indices = new List<int>();
                _viewIndices = new List<object>();

                int startIdx = series.index.FirstPositionOf(slices.start);
                int endIdx = series.index.FirstPositionOf(slices.end);

                if (slices.step > 0)
                {
                    for (int i = startIdx; i <= endIdx; i += slices.step)
                    {
                        _indices.Add(i);
                        _viewIndices.Add(series.index.GetIndex(i));
                    }
                }
                else
                {
                    for (int i = startIdx; i >= endIdx; i += slices.step)
                    {
                        _indices.Add(i);
                        _viewIndices.Add(series.index.GetIndex(i));
                    }
                }

                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            // Constructor using a list of index values
            public SeriesView(Series series, List<object> slice)
            {
                _series = series;
                _indices = new List<int>();
                _viewIndices = new List<object>();

                foreach (var item in slice)
                {
                    if (!series.index.Contains(item))
                        throw new ArgumentException($"Index {item} does not exist in the series index.");

                    foreach (var pos in series.index.GetIndexPosition(item))
                    {
                        _indices.Add(pos);
                        _viewIndices.Add(series.index.GetIndex(pos));
                    }
                }

                _viewIndexSet = new HashSet<object?>(_viewIndices);
            }

            // Factory for creating a sliced view from another view
            public SeriesView SliceView(List<object> slice)
            {
                var newIndices = new List<int>();
                var newViewIndices = new List<object>();

                var requested = new HashSet<object>(slice);

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
            /// Creates a new <see cref="SeriesView"/> by slicing the current view based on the specified range and
            /// step.
            /// </summary>
            /// <remarks>The slicing operation supports both positive and negative step values. A
            /// positive step slices elements from <paramref name="slices.start"/> to <paramref name="slices.end"/> in
            /// ascending order, while a negative step slices elements in descending order.</remarks>
            /// <param name="slices">A tuple containing the start index, end index, and step size for slicing. <paramref
            /// name="slices.start"/> and <paramref name="slices.end"/> must exist in the current view index. <paramref
            /// name="slices.step"/> specifies the interval between elements in the slice and cannot be zero.</param>
            /// <returns>A new <see cref="SeriesView"/> containing the elements from the current view that match the specified
            /// slicing criteria.</returns>
            /// <exception cref="ArgumentException">Thrown if <paramref name="slices.start"/> or <paramref name="slices.end"/> does not exist in the view
            /// index, or if <paramref name="slices.step"/> is zero.</exception>
            public SeriesView SliceView((int start, int end, int step) slices)
            {
                if (!this._viewIndexSet.Contains(slices.start) || !this._viewIndexSet.Contains(slices.end))
                    throw new ArgumentException("Start or end index does not exist in the view index.");
                if (slices.step == 0)
                    throw new ArgumentException("Step cannot be zero.");
                List<int> newIndices = new List<int>();
                List<object> newViewIndices = new List<object>();
                int startIdx = _viewIndices.IndexOf(slices.start);
                int endIdx = _viewIndices.IndexOf(slices.end);
                if (slices.step > 0)
                {
                    for (int i = startIdx; i <= endIdx; i += slices.step)
                    {
                        newIndices.Add(_indices[i]);
                        newViewIndices.Add(_viewIndices[i]);
                    }
                }
                else
                {
                    for (int i = startIdx; i >= endIdx; i += slices.step)
                    {
                        newIndices.Add(_indices[i]);
                        newViewIndices.Add(_viewIndices[i]);
                    }
                }
                return new SeriesView(_series, newIndices, newViewIndices);
            }

            /// <summary>
            /// Creates a new <see cref="Series"/> instance containing the values from the current view.
            /// </summary>
            /// <remarks>The resulting <see cref="Series"/> is constructed using the indices and
            /// values from the current view, ensuring that the data type and other metadata are preserved. The
            /// operation creates a copy of the data to ensure immutability.</remarks>
            /// <param name="name">An optional name for the resulting <see cref="Series"/>. If <paramref name="name"/> is <see
            /// langword="null"/>, the name of the original series is used.</param>
            /// <returns>A new <see cref="Series"/> containing the values from the current view, with the specified name and
            /// other properties copied from the original series.</returns>
            public Series ToSeries(string? name = null)
            {
                List<object?> values = new List<object?>(_indices.Count);
                foreach (var pos in _indices)
                {
                    values.Add(_series.valueStorage.GetValue(pos));
                }
                return new Series(values, index: _viewIndices, dtype: _series.dataType, name: name ?? _series.seriesName, copy: true);
            }

            // Indexer: return all values mapped to index, filtered by current view
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

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns a string representation of the series view, including the series name, indices, and values.
            /// </summary>
            /// <remarks>The returned string includes a formatted table with the index and
            /// corresponding value for each element in the series view. If the series name is not set, "Unnamed" is
            /// used as the default name. Values that are null are represented as "null" in the output.</remarks>
            /// <returns>A string that represents the series view, including the series name, indices, and values.</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Series View: {_series.seriesName ?? "Unnamed"}");
                sb.AppendLine("Index | Value");
                sb.AppendLine("--------------");
                for (int i = 0; i < _indices.Count; i++)
                {
                    sb.AppendLine($"{_viewIndices[i],5} | {_series.valueStorage.GetValue(_indices[i])?.ToString() ?? "null"}");
                }
                return sb.ToString();
            }

            // Count of elements in the view
            public int Count => _indices.Count;

            // Optional: expose view indices and positions for external inspection if needed

            /// <summary>
            /// Gets a read-only collection of indices. It presents the positions of the elements in the original series as integers.
            /// </summary>
            public IReadOnlyList<int> Indices => _indices;

            /// <summary>
            /// Gets a read-only collection of view indices. It presents the indices of the elements in the view original index of series as objects.
            /// </summary>
            public IReadOnlyList<object> ViewIndices => _viewIndices;
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
        public IEnumerator<object?> GetEnumerator()
        {
            return new SeriesEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="Series"/> collection.
        /// </summary>
        /// <remarks>This enumerator provides sequential access to the elements in a <see cref="Series"/>.
        /// Use <see cref="MoveNext"/> to advance the enumerator to the next element and <see cref="Current"/>  to
        /// retrieve the current element. The enumerator starts before the first element and must be  advanced before
        /// accessing elements. Once the end of the collection is reached, <see cref="MoveNext"/>  will return <see
        /// langword="false"/>.</remarks>
        private sealed class SeriesEnumerator : IEnumerator<object?>
        {
            /// <summary>
            /// Represents the series enumerator, which allows iteration over the values in a Series.
            /// </summary>
            private readonly Series _series;
            private int _currentIndex = -1;
            public SeriesEnumerator(Series series)
            {
                _series = series;
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <remarks>The value of <see cref="Current"/> is undefined until the enumerator is
            /// positioned on an element within the collection. Ensure the enumerator is properly initialized and
            /// positioned before accessing this property.</remarks>
            public object? Current => _series.valueStorage.GetValue(_currentIndex);

            /// <summary>
            /// Gets the current element in the collection being enumerated.
            /// </summary>
            object System.Collections.IEnumerator.Current => Current!;

            /// <summary>
            /// Advances the enumerator to the next element in the series.
            /// </summary>
            /// <remarks>This method increments the internal index and checks whether the index is
            /// within the bounds of the series. It should be called repeatedly to iterate through all elements in the
            /// series.</remarks>
            /// <returns><see langword="true"/> if the enumerator successfully advanced to the next element;  otherwise, <see
            /// langword="false"/> if the end of the series has been reached.</returns>
            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _series.Count;
            }

            /// <summary>
            /// Resets the internal state of the enumerator to its initial position, before the first element.
            /// </summary>
            /// <remarks>After calling this method, the enumerator must be advanced using <see
            /// cref="MoveNext">  before accessing elements.</remarks>
            public void Reset()
            {
                _currentIndex = -1;
            }

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            /// <remarks>Call this method when you are finished using the object to free up resources.
            /// After calling <see cref="Dispose"/>, the object is in an unusable state and should not be
            /// accessed.</remarks>
            public void Dispose()
            { // no operation}
            }
        }
    }
}