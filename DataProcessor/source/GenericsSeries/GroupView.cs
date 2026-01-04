using DataProcessor.source.UserSettings.DefaultValsGenerator;
using System.Reflection.Metadata.Ecma335;


namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType>
    {
        /// <summary>
        /// Provides grouped views of a <see cref="Series{DataType}"/>.
        /// </summary>
        public class GroupView
        {
            private readonly Dictionary<object, int[]> groups;
            private readonly Series<DataType> source;

            /// <summary>
            /// Initializes a new instance of the <see cref="GroupView"/> class.
            /// </summary>
            /// <param name="source">The source series.</param>
            /// <param name="groupIndices">
            /// A dictionary mapping group keys to the indices of elements in the source series.
            /// </param>
            public GroupView(Series<DataType> source, Dictionary<object, int[]> groupIndices)
            {
                this.source = source;
                this.groups = groupIndices;
            }

            /// <summary>
            /// Gets the dictionary of groups, mapping keys to element indices.
            /// </summary>
            public IReadOnlyDictionary<object, int[]> Groups => groups;


            // <summary>
            /// Gets the indices of a specific group by key.
            /// </summary>
            /// <param name="key">The group key.</param>
            public int[] this[object key] => groups[key];

            /// <summary>
            /// Returns the indices of the specified group as <see cref="ReadOnlyMemory{T}"/>.
            /// </summary>
            /// <param name="key">The group key.</param>
            /// <returns>A memory slice of indices if the group exists; otherwise, an empty memory block.</returns>
            private ReadOnlyMemory<int> GetGroupIndices(object key)
            {
                return groups.TryGetValue(key, out var indices) ? indices.AsMemory() : ReadOnlyMemory<int>.Empty;
            }

            /// <summary>
            /// Extracts a new <see cref="Series{DataType}"/> containing only the elements of a specific group.
            /// </summary>
            /// <param name="key">The group key.</param>
            /// <param name="newName">An optional name for the new series.</param>
            /// <returns>A new series containing the group values and indices.</returns>
            /// <exception cref="KeyNotFoundException">Thrown if the specified group does not exist.</exception>
            public Series<DataType> GetGroup(object key, string? newName = "")
            {
                if (!groups.TryGetValue(key, out var indices))
                    throw new KeyNotFoundException($"Group {key} does not exist.");
                var values = new List<DataType>(indices.Length);
                var indexes = new List<object>(indices.Length);
                foreach (var idx in indices)
                {
                    values.Add((DataType)this.source.values.GetValue(idx));
                    indexes.Add(this.source.index[idx]);
                }
                return new Series<DataType>(values, newName, indexes);
            }

            /// <summary>
            /// Aggregates values of each group using the specified aggregator.
            /// </summary>
            /// <param name="aggregator">The aggregator used to combine values.</param>
            /// <param name="defaultValueGenerator">Optional default value generator for initialization.</param>
            /// <returns>
            /// A dictionary mapping each group key to its aggregated value.
            /// </returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="aggregator"/> is null.</exception>
            public Dictionary<object, DataType> Sum(ICalculator<DataType>? aggregator, IDefaultValueGenerator<DataType>? defaultValueGenerator = null)
            {
                if (aggregator == null)
                    throw new ArgumentException("Aggregator cannot be null. Please provide a valid aggregator.");

                var result = new Dictionary<object, DataType>();
                foreach (var kvp in groups)
                {
                    object key = kvp.Key;
                    int[] indices = kvp.Value;
                    DataType sum = defaultValueGenerator != null ? defaultValueGenerator.GenerateDefaultValue() : default;

                    foreach (var idx in indices)
                    {
                        if (this.source.values[idx] != null)
                        {
                            sum = aggregator.Add(sum, (DataType)this.source.values.GetValue(idx));
                        }
                    }
                    result[key] = sum;
                }

                return result;
            }
            /// <summary>
            /// Counts the number of elements in each group.
            /// </summary>
            /// <returns>
            /// A dictionary mapping each group key to the number of elements it contains.
            /// </returns>
            public Dictionary<object, uint> Count()
            {
                var result = new Dictionary<object, uint>();
                foreach (var kvp in groups)
                {
                    result[kvp.Key] = (uint)kvp.Value.Length;
                }
                return result;
            }

            /// <summary>
            /// Calculates the mean for each group.
            /// </summary>
            public Dictionary<object, double> Mean(Func<DataType, double> selector)
            {
                var result = new Dictionary<object, double>();
                foreach (var kvp in groups)
                {
                    var values = kvp.Value
                        .Select(idx => selector((DataType)this.source.values.GetValue(idx)))
                        .ToList();

                    result[kvp.Key] = values.Count > 0 ? values.Average() : double.NaN;
                }
                return result;
            }

            /// <summary>
            /// Finds the minimum value in each group.
            /// </summary>
            public Dictionary<object, DataType> Min(Comparer<DataType>? comparer = null)
            {
                comparer ??= Comparer<DataType>.Default;
                var result = new Dictionary<object, DataType>();
                foreach (var kvp in groups)
                {
                    var vals = kvp.Value.Select(idx => (DataType)this.source.values.GetValue(idx));
                    result[kvp.Key] = vals.Min();
                }
                return result;
            }

            /// <summary>
            /// Finds the maximum value in each group.
            /// </summary>
            public Dictionary<object, DataType> Max(Comparer<DataType>? comparer = null)
            {
                comparer ??= Comparer<DataType>.Default;
                var result = new Dictionary<object, DataType>();
                foreach (var kvp in groups)
                {
                    var vals = kvp.Value.Select(idx => (DataType)this.source.values.GetValue(idx));
                    result[kvp.Key] = vals.Max();
                }
                return result;
            }


            /// <summary>
            /// Apply a custom function to each group and return results in dictionary.
            /// </summary>
            public Dictionary<object, TResult> Apply<TResult>(Func<Series<DataType>, TResult> func)
            {
                var result = new Dictionary<object, TResult>();
                foreach (var kvp in groups)
                {
                    var series = GetGroup(kvp.Key);
                    result[kvp.Key] = func(series);
                }
                return result;
            }

            /// <summary>
            /// Transform each value in groups and return a new Series.
            /// </summary>
            public Series<DataType> Transform(Func<DataType, DataType> transformer, string? newName = "")
            {
                var newValues = new List<DataType>();
                var newIndexes = new List<object>();

                foreach (var kvp in groups)
                {
                    foreach (var idx in kvp.Value)
                    {
                        var val = (DataType)this.source.values.GetValue(idx);
                        newValues.Add(transformer(val));
                        newIndexes.Add(this.source.index[idx]);
                    }
                }
                return new Series<DataType>(newValues, newName, newIndexes);
            }

            // <summary>
            /// Lọc ra những nhóm thỏa predicate (giống groupby.filter trong Pandas)
            /// </summary>
            public GroupView Filter(Func<Series<DataType>, bool> predicate)
            {
                var filtered = groups
                    .Where(kvp => predicate(GetGroup(kvp.Key)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return new GroupView(source, filtered);
            }

            /// <summary>
            /// Apply trả về Series, flatten kết quả như Pandas.
            /// </summary>
            public Series<DataType> Apply(Func<Series<DataType>, Series<DataType>> func)
            {
                var newValues = new List<DataType>();
                var newIndex = new List<object>();

                foreach (var kvp in groups)
                {
                    var subgroup = GetGroup(kvp.Key);
                    var result = func(subgroup);

                    newValues.AddRange(result.Values);
                    newIndex.AddRange(result.Index);
                }

                return new Series<DataType>(newValues, index: newIndex);
            }

           /// <summary>
           /// Applies the provided aggregation functions to each group and returns the results.
           /// </summary>
           /// <param name="aggregations">
           /// A dictionary mapping aggregation names to functions. Each function is invoked with a
           /// <see cref="Series{DataType}"/> representing a group and should return the aggregation result.
           /// </param>
           /// <returns>
           /// A dictionary that maps each group key to another dictionary. The inner dictionary maps
           /// aggregation names to the computed aggregation result for that group.
           /// </returns>
           /// <exception cref="ArgumentNullException">Thrown if <paramref name="aggregations"/> is null.</exception>
            public Dictionary<object, Dictionary<string, object>> Aggregate(
                Dictionary<string, Func<Series<DataType>, object>> aggregations)
            {
                var result = new Dictionary<object, Dictionary<string, object>>();

                foreach (var kvp in groups)
                {
                    var groupSeries = GetGroup(kvp.Key);
                    var aggResult = new Dictionary<string, object>();

                    foreach (var (name, func) in aggregations)
                    {
                        aggResult[name] = func(groupSeries);
                    }

                    result[kvp.Key] = aggResult;
                }

                return result;
            }
            // ------------------------- Access Helpers ---------------------------- //

            /// <summary>
            /// Retrieves the first element associated with the specified key.
            /// </summary>
            /// <param name="key">The key used to locate the group of elements.</param>
            /// <returns>The first element in the group associated with the specified key.</returns>
            /// <exception cref="KeyNotFoundException">Thrown if the specified <paramref name="key"/> does not exist in the collection  or if the group
            /// associated with the key is empty.</exception>
            public DataType First(object key)
            {
                if (!groups.TryGetValue(key, out var indices) || indices.Length == 0)
                    throw new KeyNotFoundException($"Nhóm {key} không tồn tại hoặc rỗng.");
                return (DataType)this.source.values.GetValue(indices[0]);
            }

            /// <summary>
            /// Retrieves the last element in the group associated with the specified key.
            /// </summary>
            /// <param name="key">The key identifying the group from which to retrieve the last element.</param>
            /// <returns>The last element in the group associated with the specified key.</returns>
            /// <exception cref="KeyNotFoundException">Thrown if the specified <paramref name="key"/> does not exist in the collection or if the group is
            /// empty.</exception>
            public DataType Last(object key)
            {
                if (!groups.TryGetValue(key, out var indices) || indices.Length == 0)
                    throw new KeyNotFoundException($"Group {key} does not exist or empty.");
                return (DataType)this.source.values.GetValue(indices[^1]);
            }

            /// <summary>
            /// Retrieves the nth element from a group identified by the specified key.
            /// </summary>
            /// <param name="key">The key identifying the group from which to retrieve the element.</param>
            /// <param name="n">The zero-based index of the element to retrieve within the group.</param>
            /// <returns>The element at the specified index within the group.</returns>
            /// <exception cref="IndexOutOfRangeException">Thrown if the group identified by <paramref name="key"/> does not exist or does not contain enough
            /// elements to satisfy the specified index <paramref name="n"/>.</exception>
            public DataType Nth(object key, int n)
            {
                if (!groups.TryGetValue(key, out var indices) || indices.Length <= n)
                    throw new IndexOutOfRangeException($"Group {key} does not have {n}.");
                return (DataType)this.source.values.GetValue(indices[n]);
            }
        }

    }
}
