namespace DataProcessor.source.Index
{
    public class DecimalIndex : IIndex
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

        public override IIndex Slice(int start, int end, int step = 1)
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
    }
}
