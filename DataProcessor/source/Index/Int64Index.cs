namespace DataProcessor.source.Index
{
    public class Int64Index : IIndex
    {
        private new readonly List<long> indexList;
        private new readonly Dictionary<long, List<int>> indexMap;

        public Int64Index(List<long> indexList)
            : base(indexList.Cast<object>().ToList())
        {
            this.indexList = indexList;
            indexMap = new Dictionary<long, List<int>>();

            // Xây dựng dictionary ánh xạ giữa index và các vị trí
            for (int i = 0; i < indexList.Count; i++)
            {
                long key = indexList[i];
                if (!indexMap.ContainsKey(key))
                {
                    indexMap[key] = new List<int>();
                }
                indexMap[key].Add(i);
            }
        }

        // private methods
        private void RebuildMap()
        {
            indexMap.Clear();
            for (int i = 0; i < indexList.Count; i++)
            {
                long key = indexList[i];
                if (!indexMap.ContainsKey(key))
                    indexMap[key] = new List<int>();
                indexMap[key].Add(i);
            }
        }

        // protected methods
        protected override void Add(object key)
        {
            if (key is long intKey)
            {
                indexList.Add(intKey);
                if (!indexMap.ContainsKey(intKey))
                {
                    indexMap[intKey] = new List<int>();
                }
                indexMap[intKey].Add(Count - 1);
            }

        }

        // Lấy giá trị index tại vị trí idx
        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        // Lấy tất cả các vị trí của một giá trị index
        public override List<int> GetIndexPosition(object index)
        {
            if (index is long idxValue && indexMap.ContainsKey(idxValue))
            {
                return indexMap[idxValue];
            }
            throw new KeyNotFoundException($"Index {index} not found.");
        }

        // Phương thức slice để lấy một phần của index
        public override IIndex Slice(int start, int end, int step)
        {
            List<object> slicedIndex = new List<object>();
            if(step == 0)
            {
                throw new ArgumentException("step must not be 0");
            }
            // Kiểm tra điều kiện bước nhảy âm
            if (step > 0)
            {
                for (int i = start; i < end; i += step)
                {
                    slicedIndex.Add(indexList[i]);
                }
            }
            else
            {
                for (int i = start; i > end; i += step)
                {
                    slicedIndex.Add(indexList[i]);
                }
            }

            return new Int64Index(slicedIndex.Cast<long>().ToList());  // Trả về Int64Index với List<long>
        }

        protected override void Drop(object key)
        {
            if (key is not long longKey || !indexMap.ContainsKey(longKey))
                return;

            var positions = indexMap[longKey];
            foreach (var pos in positions.OrderByDescending(p => p))
            {
                indexList.RemoveAt(pos);
            }

            indexMap.Remove(longKey);

            // Cập nhật lại indexMap vì vị trí các phần tử phía sau đã thay đổi
            RebuildMap();
        }

    }
}
