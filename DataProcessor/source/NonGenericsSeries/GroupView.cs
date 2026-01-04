using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series: ISeries
    {
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

            public Dictionary<object, object> Apply(Func<Series, object> func)
            {
                var result = new Dictionary<object, object>();
                foreach(var kvp in _groups)
                {
                    var series = new Series(this[kvp.Key]);
                    result[kvp.Key] = func(series);
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
    }
}
