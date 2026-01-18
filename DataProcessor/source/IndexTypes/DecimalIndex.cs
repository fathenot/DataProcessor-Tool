namespace DataProcessor.source.IndexTypes
{
    public class DecimalIndex : DataIndex
    {
        List<decimal> decimals;
        Dictionary<decimal, List<int>> indexMap;

        public DecimalIndex(List<decimal> decimals) : base(decimals.Cast<object>().ToList())
        {
            this.decimals = decimals;
            indexMap = new Dictionary<decimal, List<int>>();
            for (int i = 0; i < decimals.Count; i++)
            {
                if (!indexMap.ContainsKey(decimals[i]))
                {
                    indexMap[decimals[i]] = new List<int>();
                }
                indexMap[decimals[i]].Add(i);
            }
        }

        public override int Count => decimals.Count;
        public override IReadOnlyList<object> IndexList => decimals.Cast<object>().ToList().AsReadOnly();

        public override IReadOnlyList<int> GetIndexPosition(object decimalValue)
        {
            if (decimalValue is decimal dec && indexMap.ContainsKey(dec))
            {
                return indexMap[dec];
            }
            throw new KeyNotFoundException($"Decimal {decimalValue} not found");
        }

        public override bool Contains(object key)
        {
            return key is decimal dec && indexMap.ContainsKey(dec);
        }

        public override object GetIndex(int idx)
        {
            return decimals[idx];
        }

        public override bool Equals(object? obj)
        {
            if (obj is DecimalIndex other)
            {
                return this.decimals.SequenceEqual(other.decimals);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return decimals.Aggregate(0, (current, dec) => current ^ dec.GetHashCode());
        }

        public override int FirstPositionOf(object key)
        {
            if (key is decimal dec && indexMap.ContainsKey(dec))
            {
                return indexMap[dec].FirstOrDefault();
            }
            return -1; // Not found
        }

        /// <summary>
        /// Creates a new index by extracting a subset of elements from the current index,  based on the specified
        /// start, end, and step parameters. It will included the elements at the start and end indices
        /// </summary>
        /// <remarks>The method supports both forward and reverse slicing based on the sign of <paramref
        /// name="step"/>. If <paramref name="step"/> is positive, elements are selected from <paramref name="start"/>
        /// to <paramref name="end"/> inclusively. If <paramref name="step"/> is negative, elements are selected in
        /// reverse order from <paramref name="start"/> to <paramref name="end"/> inclusively.</remarks>
        /// <param name="start">The zero-based starting index of the slice. Must be within the bounds of the current index.</param>
        /// <param name="end">The zero-based ending index of the slice. Must be within the bounds of the current index.</param>
        /// <param name="step">The step size for the slice. Must not be zero. Positive values iterate forward, while negative values
        /// iterate backward.</param>
        /// <returns>A new <see cref="Index"/> containing the elements from the current index that match the specified slicing
        /// criteria.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> or <paramref name="end"/> is outside the bounds of the current index.</exception>

        public override DataIndex Slice(int start, int end, int step = 1)
        {
            if (step == 0)
            {
                throw new ArgumentException("Step cannot be zero.", nameof(step));
            }

            if (start >= decimals.Count || end >= decimals.Count || start < 0 || end < 0)
            {
                throw new ArgumentOutOfRangeException("Start or end index is out of range.");
            }

            List<decimal> slicedDecimals = new List<decimal>();
            if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    slicedDecimals.Add(decimals[i]);
                }
            }
            else
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedDecimals.Add(decimals[i]);
                }
            }

            return new DecimalIndex(slicedDecimals);

        }

        /// <summary>
        /// Creates a new index by extracting elements from the current index based on the specified list of keys.
        /// </summary>
        /// <remarks>This method iterates through the provided list of keys and retrieves all elements
        /// associated with each key  from the current index. If a key is invalid or missing, an exception is
        /// thrown.</remarks>
        /// <param name="indexList">A list of objects representing the keys to slice the index. Each key must be a <see langword="decimal"/> 
        /// and must exist in the current index.</param>
        /// <returns>A new <see cref="DecimalIndex"/> containing the elements corresponding to the specified keys.</returns>
        /// <exception cref="ArgumentException">Thrown if any key in <paramref name="indexList"/> is not a <see langword="decimal"/> or does not exist in
        /// the current index.</exception>
        public override DataIndex TakeKeys(List<object> indexList)
        {
            List<decimal> slicedDecimals = new List<decimal>();
            foreach (var item in indexList)
            {
                if (item is decimal dec && indexMap.ContainsKey(dec))
                {
                    foreach (var index in indexMap[dec])
                    {
                        slicedDecimals.Add(dec);
                    }
                }
                else
                {
                    throw new ArgumentException($"Index {item} not found in the current index.");
                }
            }
            return new DecimalIndex(slicedDecimals);
        }
        public override IEnumerable<object> DistinctIndices()
        {
            return decimals.Distinct().Cast<object>().ToList();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (var dec in decimals)
            {
                yield return dec;
            }
        }

        public override object this[int index]
        {
            protected set
            {
                if (index < 0 || index >= decimals.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                decimal oldValue = decimals[index];
                decimals[index] = (decimal)value;

                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (!indexMap[oldValue].Any())
                    {
                        indexMap.Remove(oldValue);
                    }
                }

                if (!indexMap.ContainsKey((decimal)value))
                {
                    indexMap[(decimal)value] = new List<int>();
                }
                indexMap[(decimal)value].Add(index);
            }
        }

        public override DataIndex Clone()
        {
            return new DecimalIndex(decimals);
        }
    }
}
