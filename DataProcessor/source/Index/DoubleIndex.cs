
namespace DataProcessor.source.Index
{
    public class DoubleIndex : IIndex
    {
        private new readonly List<double> indexList;
        private new readonly Dictionary<double, List<int>> indexMap;

        // constructor
        public DoubleIndex()
        {
            indexList = new List<double>();
            indexMap = new Dictionary<double, List<int>>();
        }

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
                else
                {
                    indexMap[index[i]].Add(i);
                }
            }
        }

        // private method
        private void RebuildMap()
        {
            indexMap.Clear();
            for (int i = 0; i < indexList.Count; i++)
            {
                double key = indexList[i];
                if (!indexMap.ContainsKey(key))
                    indexMap[key] = new List<int>();
                indexMap[key].Add(i);
            }
        }

        // protected method
        protected override void Drop(object key)
        {
            if (key is not double doubleKey || !indexMap.ContainsKey(doubleKey))
                return;

            var positions = indexMap[doubleKey];
            foreach (var pos in positions.OrderByDescending(p => p))
            {
                indexList.RemoveAt(pos);
            }

            indexMap.Remove(doubleKey);

            // Cập nhật lại indexMap vì vị trí các phần tử phía sau đã thay đổi
            RebuildMap();
        }

        protected override void Add(object key)
        {
            if (key is double doubleKey)
            {
                if (!indexMap.ContainsKey(doubleKey))
                {
                    indexMap[doubleKey] = new List<int>();
                }
                indexList.Add(doubleKey);
                indexMap[doubleKey].Add(Count - 1);
            }
        }

        //public and internal methods
        public override DoubleIndex Slice(int start, int end, int step)
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

        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            if (index is double doubleKey && indexMap.ContainsKey(doubleKey))
            {
                return indexMap[doubleKey];
            }
            throw new KeyNotFoundException($"Index {index} not found");
        }
    }
}
