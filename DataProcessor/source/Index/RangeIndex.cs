using System.Collections;

namespace DataProcessor.source.Index
{
    public class RangeIndex : IIndex
    {
        private readonly int _start;
        private readonly int _stop;
        private readonly int _step;

        // Constructor
        public RangeIndex(int start, int stop, int step = 1)
        {
            _start = start;
            _stop = stop;
            _step = step;

        }


        public int Start => _start;
        public int Stop => _stop;
        public int Step => _step;


        /// <summary>
        /// Gets the number of elements in the sequence represented by this instance.
        /// </summary>
        public override int Count
        {
            get
            {
                if(_step > 0)
                {
                    return Math.Abs(_stop - _start) / _step + 1; // Calculate count for positive step
                }
                else
                {
                    return Math.Abs(_start - _stop) / Math.Abs(_step) + 1; // Calculate count for negative step
                }
            }
        }
        public override IReadOnlyList<object> IndexList
        {
            get
            {
                return this.DistinctIndices().ToList();
            }
        }


        /// <summary>
        /// Creates a new index by slicing the current range with the specified start, end, and step values.
        /// </summary>
        /// <remarks>The method calculates the new start, end, and step values based on the current range
        /// and the provided parameters. The resulting slice is represented as a new <see cref="RangeIndex"/>
        /// instance.</remarks>
        /// <param name="start">The zero-based starting index of the slice, relative to the current range.</param>
        /// <param name="end">The zero-based ending index of the slice, relative to the current range. The slice will exclude this index.</param>
        /// <param name="step">The step size for the slice. Defaults to 1. Must be a non-zero value.</param>
        /// <returns>A new <see cref="IIndex"/> representing the sliced range.</returns>
        public override IIndex Slice(int start, int end, int step = 1)
        {
            if (step == 0)
                throw new ArgumentException("Step must not be zero.", nameof(step));
            // Tính toán start, stop mới cho slice
            int actualStart = _start + start * _step;
            int actualStop = _start + end * _step;
            int combinedStep = _step * step;

            // Trả về một RangeIndex mới đã slice
            return new RangeIndex(actualStart, actualStop, combinedStep);
        }

        public override IIndex Slice(List<object> indexList)
        {
            if (indexList.Cast<int>().Any(v => v < _start || v > this._stop))
            {
                throw new ArgumentOutOfRangeException(nameof(indexList), "Index list contains values outside the range of the current index.");
            }

            return new Int64Index(indexList.Cast<long>().ToList());
        }

        /// <summary>
        /// Calculates the value at the specified index in a sequence.
        /// </summary>
        /// <param name="idx">The zero-based index of the desired value in the sequence.</param>
        /// <returns>The value at the specified index, calculated as the starting value plus the index multiplied by the step
        /// size.</returns>
        public override object GetIndex(int idx)
        {
            if (idx < 0 || idx >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(idx), "Index is out of range.");
            }
            return _start + idx * _step;
        }

        /// <summary>
        /// gets the positions of the specified index in the sequence.
        /// </summary>
        /// <param name="index">the specified index </param>
        /// <returns>a list contains positions of specified index (in this case is the index)</returns>
        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            return new List<int> { FirstPositionOf(index) };
        }

        /// <summary>
        /// Determines the zero-based position of the specified key relative to the starting value.
        /// </summary>
        /// <param name="key">The key to locate, which must be convertible to an integer.</param>
        /// <returns>The first zero-based position of the key as an integer, calculated by subtracting the starting value.</returns>
        public override int FirstPositionOf(object key)
        {
            return this.Contains(key)? (Convert.ToInt32(key) - _start) / _step : -1;
        }

        /// <summary>
        /// Determines whether the specified key exists in the collection.
        /// </summary>
        /// <param name="key">The key to locate in the collection. Must be convertible to an integer.</param>
        /// <returns><see langword="true"/> if the key exists in the collection; otherwise, <see langword="false"/>.</returns>
        public override bool Contains(object key)
        {
            var tmp = Convert.ToInt32(key);
            if(_step > 0)
            {
                return tmp >= _start && tmp <= _stop && (tmp - _start) % _step == 0;
            }
            else
            {
                return tmp <= _start && tmp >= _stop && (_start - tmp) % Math.Abs(_step) == 0;
            }

        }

        /// <summary>
        /// Returns a collection of distinct indices generated based on the current range configuration.
        /// </summary>
        /// <remarks>The indices are generated by iterating from the starting value to the stopping value,
        /// incrementing by the step value. Each index is converted to an integer before being added  to the collection.
        /// The resulting collection contains unique indices in the specified range.</remarks>
        /// <returns>An <see cref="IEnumerable{Object}"/> containing the distinct indices as integers.</returns>
        public override IEnumerable<object> DistinctIndices()
        {
            List<object> tmp = new List<object>();
            for (int i = _start; i != _stop; i += _step)
            {
                tmp.Add(Convert.ToInt32(i));
            }
            return tmp;
        }

        public override IEnumerator<object> GetEnumerator()
        {
           if(_step > 0)
            {
                for (int i = _start; i <= _stop; i += _step)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = _start; i >= _stop; i += _step)
                {
                    yield return i;
                }
            }
        }
    }
}
