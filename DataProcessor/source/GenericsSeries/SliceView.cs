using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType>
    {
        /// <summary>
        /// Represents a view of a subset of data within a <see cref="Series{DataType}"/>.
        /// </summary>
        public class SeriesView
        {
            private readonly Series<DataType> series;
            private List<object> indices;
            private List<int> positionList;
            private HashSet<object> indexSet; // for fast lookup

            #region Constructors
            private SeriesView(Series<DataType> series, IEnumerable<object> indices, IEnumerable<int> positions)
            {
                this.series = series ?? throw new ArgumentNullException(nameof(series));
                this.indices = indices?.ToList() ?? throw new ArgumentNullException(nameof(indices));
                this.positionList = positions?.ToList() ?? throw new ArgumentNullException(nameof(positions));

                if (!this.indices.Any())
                    throw new ArgumentException("Index cannot be empty.", nameof(indices));

                this.indexSet = new HashSet<object>(this.indices);
            }

            public SeriesView(Series<DataType> series, List<object> indices)
            {
                this.series = series ?? throw new ArgumentNullException(nameof(series));
                if (indices == null || indices.Count == 0)
                    throw new ArgumentException("Index cannot be null or empty", nameof(indices));

                this.indices = new List<object>();
                this.positionList = new List<int>();

                foreach (var idx in indices)
                {
                    if (!series.index.Contains(idx))
                        throw new ArgumentException($"Index {idx} does not exist in the series.", nameof(indices));

                    this.indices.Add(idx);
                    this.positionList.AddRange(series.index.GetIndexPosition(idx));
                }

                this.indexSet = new HashSet<object>(this.indices);
            }

            public SeriesView(Series<DataType> series, (object start, object end, int step) slice)
            {
                this.series = series ?? throw new ArgumentNullException(nameof(series));
                if (slice.step == 0) throw new ArgumentException("Step must not be 0.", nameof(slice.step));

                if (!series.index.Contains(slice.start) || !series.index.Contains(slice.end))
                    throw new ArgumentOutOfRangeException("Start or end index does not exist in the series.");

                int startPos = series.index.FirstPositionOf(slice.start);
                int endPos = series.index.FirstPositionOf(slice.end);

                this.indices = new List<object>();
                this.positionList = new List<int>();
                this.indexSet = new HashSet<object>();

                if (slice.step > 0)
                {
                    for (int i = startPos; i <= endPos; i += slice.step)
                        AddToView(i);
                }
                else
                {
                    for (int i = startPos; i >= endPos; i += slice.step)
                        AddToView(i);
                }
            }
            #endregion

            #region Helpers
            private void AddToView(int position)
            {
                this.indices.Add(series.index[position]);
                this.positionList.Add(position);
                this.indexSet.Add(series.index[position]);
            }
            #endregion

            #region Public API

            /// <summary>
            /// Creates a new <see cref="SeriesView"/> containing only the elements
            /// whose indices are included in the specified subset.
            /// </summary>
            /// <param name="subset">
            /// A list of index values to extract from the current view. 
            /// Each index must exist in the current <see cref="SeriesView"/>.
            /// </param>
            /// <returns>
            /// A new <see cref="SeriesView"/> containing the elements
            /// corresponding to the provided indices.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="subset"/> is <c>null</c>, empty,
            /// or contains an index that does not exist in the current view.
            /// </exception>
            public SeriesView GetView(List<object> subset)
            {
                if (subset == null || subset.Count == 0)
                    throw new ArgumentException("Subset cannot be null or empty.", nameof(subset));

                var newIndices = new List<object>();
                var newPositions = new List<int>();

                foreach (var idx in subset)
                {
                    if (!this.indexSet.Contains(idx))
                        throw new ArgumentException($"Index {idx} does not exist in the current view.", nameof(subset));

                    newIndices.Add(idx);

                    foreach (var pos in this.series.index.GetIndexPosition(idx))
                    {
                        if (this.positionList.Contains(pos))
                            newPositions.Add(pos);
                    }
                }

                return new SeriesView(this.series, newIndices, newPositions);
            }


            /// <summary>
            /// Creates a new <see cref="SeriesView"/> by slicing the current view
            /// from a start index to an end index, with the specified step size.
            /// </summary>
            /// <param name="slice">
            /// A tuple (<c>start</c>, <c>end</c>, <c>step</c>) defining the slice parameters:
            /// <list type="bullet">
            /// <item><c>start</c> — The starting index (inclusive).</item>
            /// <item><c>end</c> — The ending index (inclusive).</item>
            /// <item><c>step</c> — The step between elements. Must not be 0.</item>
            /// </list>
            /// </param>
            /// <returns>
            /// A new <see cref="SeriesView"/> containing the sliced elements
            /// based on the given range and step.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <c>step</c> is 0, or when <c>start</c> or <c>end</c>
            /// does not exist in the current view.
            /// </exception>
            public SeriesView GetView((object start, object end, int step) slice)
            {
                if (slice.step == 0)
                    throw new ArgumentException("Step must not be 0.", nameof(slice.step));
                if (!this.indexSet.Contains(slice.start))
                    throw new ArgumentException("Start does not exist in the current view.");
                if (!this.indexSet.Contains(slice.end))
                    throw new ArgumentException("End does not exist in the current view.");

                var startPositions = GetPositions(slice.start);
                var endPositions = GetPositions(slice.end);

                var newIndices = new List<object>();
                var newPositions = new HashSet<int>();

                for (int k = 0; k < Math.Min(startPositions.Count, endPositions.Count); k++)
                {
                    int start = startPositions[k];
                    int end = endPositions[k];

                    if (slice.step > 0)
                    {
                        for (int i = start; i <= end; i += slice.step)
                            if (newPositions.Add(i)) newIndices.Add(this.indices[i]);
                    }
                    else
                    {
                        for (int i = start; i >= end; i += slice.step)
                            if (newPositions.Add(i)) newIndices.Add(this.indices[i]);
                    }
                }

                return new SeriesView(this.series, newIndices, newPositions);
            }
            /// <summary>
            /// Finds all positions in the current view that match the specified index value.
            /// </summary>
            /// <param name="target">
            /// The index value to search for within the current view.
            /// </param>
            /// <returns>
            /// A list of zero-based positions where the index equals <paramref name="target"/>.
            /// </returns>
            public List<int> GetPositions(object target)
            {
                return this.indices
                           .Select((val, idx) => (val, idx))
                           .Where(p => Equals(p.val, target))
                           .Select(p => p.idx)
                           .ToList();
            }

            /// <summary>
            /// Converts the current <see cref="SeriesView"/> into a full <see cref="Series{T}"/> instance.
            /// </summary>
            /// <param name="name">
            /// An optional name for the resulting series. 
            /// If not provided, the series will be unnamed.
            /// </param>
            /// <returns>
            /// A new <see cref="Series{T}"/> containing the values and indices 
            /// of the current <see cref="SeriesView"/>.
            /// </returns>
            public Series<DataType> ToSeries(string? name = null)
            {
                var values = this.positionList
                              .Select(pos => (DataType)this.series.values.GetValue(pos))
                              .ToList();

                return new Series<DataType>(values, name, new List<object>(this.indices));
            }

            /// <summary>
            /// Gets the values in the current <see cref="SeriesView"/> as a read-only list.
            /// </summary>
            public IReadOnlyList<DataType> Values =>
                this.positionList.Select(pos => this.series.Values[pos]).ToList();

            /// <summary>
            /// Returns an enumerator that iterates through the values in the current <see cref="SeriesView"/>.
            /// </summary>
            /// <returns>
            /// An enumerator for the values in the view.
            /// </returns>
            public IEnumerator<DataType> GetValueEnumerator() =>
                this.positionList.Select(pos => (DataType)this.series.values.GetValue(pos)).GetEnumerator();

            /// <summary>
            /// Returns an enumerator that iterates through the indices in the current <see cref="SeriesView"/>.
            /// </summary>
            /// <returns>
            /// An enumerator for the indices in the view.
            /// </returns>
            public IEnumerator<object> GetIndexEnumerator() =>
                this.indices.GetEnumerator();

            /// <summary>
            /// Returns a string representation of the current <see cref="SeriesView"/>.
            /// </summary>
            /// <returns>
            /// A formatted string that displays the name of the underlying series, 
            /// along with its indices and values.
            /// </returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"SeriesView of {series.Name ?? "Unnamed Series"}");
                sb.AppendLine("Index | Value");
                sb.AppendLine("--------------");

                for (int i = 0; i < this.positionList.Count; i++)
                {
                    var idx = this.indices[i];
                    var val = this.series.values.GetValue(this.positionList[i]);
                    sb.AppendLine($"{idx,5} | {val?.ToString() ?? "null"}");
                }

                return sb.ToString();
            }
            #endregion
        }
    }
}
