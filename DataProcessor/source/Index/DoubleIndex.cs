namespace DataProcessor.source.Index
{
    public class DoubleIndex : IIndex
    {
        private readonly List<double> indexList;
        private readonly Dictionary<double, List<int>> indexMap;

        private static double ConvertToDouble(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid index: cannot convert {value} to double.", ex);
            }
        }
        // constructor
        public DoubleIndex(List<double> index) : base(index.Cast<object>().ToList())
        {
            indexList = index;
            indexMap = new Dictionary<double, List<int>>();
            for (int i = 0; i < index.Count; i++)
            {
                if (!indexMap.ContainsKey(index[i]))
                {
                    indexMap[index[i]] = new List<int>();
                }

                indexMap[index[i]].Add(i);
            }
        }

        public override int Count => indexList.Count;

        public override IReadOnlyList<object> IndexList => indexList.Cast<object>().ToList().AsReadOnly();

        //public and internal methods
        public override IIndex Slice(int start, int end, int step)
        {
            List<double> slicedIndex = new List<double>();

            if (step == 0)
            {
                throw new ArgumentException($"step must not be 0");
            }
            else if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    slicedIndex.Add(indexList[i]);
                }
            }
            else
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedIndex.Add(indexList[i]);
                }
            }
            return new DoubleIndex(slicedIndex);
        }

        public override IIndex Slice(List<object> indexList)
        {
            List<double> slicedIndex = new List<double>();
            foreach (var item in indexList)
            {
                var convertedValue = ConvertToDouble(item);
                if (!indexMap.ContainsKey(convertedValue))
                {
                    throw new ArgumentException($"Index {item} not found in the current index.");
                }
                foreach (var position in indexMap[convertedValue])
                {
                    slicedIndex.Add(convertedValue);
                }
            }
            return new DoubleIndex(slicedIndex);
        }
        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        public override bool Contains(object key)
        {
            double tmp = ConvertToDouble(key);
            return indexMap.ContainsKey(tmp);
        }

        public override int FirstPositionOf(object key)
        {
            double tmp = ConvertToDouble(key);
            if (indexMap.ContainsKey(tmp))
                return indexMap[tmp][0];
            return -1;
        }

        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            if (index is double doubleKey && indexMap.ContainsKey(doubleKey))
            {
                return indexMap[doubleKey];
            }
            throw new KeyNotFoundException($"Index {index} not found");
        }

        public override IEnumerable<object> DistinctIndices()
        {
            return indexList.Distinct().Cast<object>();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (object item in indexList)
            {
                yield return item;
            }
        }

        public override object this[int index]
        {
            protected set
            {
                if (index < 0 || index >= indexList.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                double oldValue = indexList[index];
                indexList[index] = (double)value;

                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (indexMap[oldValue].Count == 0)
                    {
                        indexMap.Remove(oldValue);
                    }
                }

                if (!indexMap.ContainsKey((double)value))
                {
                    indexMap[(double)value] = new List<int>();
                }
                indexMap[(double)value].Add(index);
            }
        }

        public override IIndex Clone()
        {
            return new DoubleIndex(indexList);
        }
    }
}
