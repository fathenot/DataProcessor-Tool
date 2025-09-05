using DataProcessor.source.UserSettings.DefaultValsGenerator;
using System.Text.RegularExpressions;


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
                    DataType sụm = defaultValueGenerator != null ? defaultValueGenerator.GenerateDefaultValue() : default;

                    foreach (var idx in indices)
                    {
                        if (this.source.values[idx] != null)
                        {
                            sụm = aggregator.Add(sụm, (DataType)this.source.values.GetValue(idx));
                        }
                    }
                    result[key] = sụm;
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
        }


    }
}
