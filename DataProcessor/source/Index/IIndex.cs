namespace DataProcessor.source.Index
{
    public abstract class IIndex
    {
        protected readonly List<object> indexList;
        protected readonly Dictionary<object, List<int>> indexMap;

        protected IIndex()
        {
            indexList = new List<object>();
            indexMap = new Dictionary<object, List<int>>();
        }
        protected IIndex(List<object> indexList)
        {
            this.indexList = indexList;
            indexMap = new Dictionary<object, List<int>>();

            // Xây dựng dictionary ánh xạ giữa index và các vị trí
            for (int i = 0; i < indexList.Count; i++)
            {
                var key = indexList[i];
                if (!indexMap.TryGetValue(key, out var positions))
                {
                    positions = new List<int>();
                    indexMap[key] = positions;
                }
                positions.Add(i);
            }
        }

        protected virtual void Drop(object key)
        {
            if (!indexMap.ContainsKey(key))
                return;

            var positions = indexMap[key];

            // Xóa tất cả vị trí khớp trong indexList
            foreach (var pos in positions.OrderByDescending(p => p))
            {
                indexList.RemoveAt(pos);
            }

            // Xóa luôn ánh xạ key
            indexMap.Remove(key);
        }

        public virtual object GetIndex(int idx)
        {
            return indexList[idx];
        }

        public virtual IReadOnlyList<int> GetIndexPosition(object index)
        {
            if (indexMap.ContainsKey(index))
            {
                return indexMap[index];
            }
            throw new KeyNotFoundException($"Index {index} not found");
        }

        protected virtual void Add(object key)
        {
            int pos = indexList.Count;
            indexList.Add(key);

            if (!indexMap.TryGetValue(key, out var positions))
            {
                positions = new List<int>();
                indexMap[key] = positions;
            }
            positions.Add(pos);
        }


        public int Count => indexList.Count;
        protected IReadOnlyList<object> IndexList => indexList;

        public abstract IIndex Slice(int start, int end, int step = 1);

        public bool Contains(object key) => indexMap.ContainsKey(key);

        public int FirstPositionOf(object key) =>
            indexMap.TryGetValue(key, out var list) && list.Count > 0
                ? list[0]
                : throw new KeyNotFoundException($"Index {key} not found");

        public IEnumerable<object> DistinctIndices() => indexMap.Keys;

        public IEnumerator<object> GetEnumerator() => indexList.GetEnumerator();

    }

}
