﻿namespace DataProcessor.source.Index
{
    public class Int64Index : IIndex
    {
        private readonly List<long> indexList;
        private readonly Dictionary<long, List<int>> indexMap;

        private long ConvertToLong(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            try
            {
                return Convert.ToInt64(key);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid index: cannot convert {key} to long.", ex);
            }
        }
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

        public override int Count => indexList.Count;

        public override IReadOnlyList<object> IndexList => indexList.Cast<object>().ToList().AsReadOnly();
        // Lấy giá trị index tại vị trí idx
        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        public override int FirstPositionOf(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var tmp = ConvertToLong(key);
            if (indexMap.TryGetValue(tmp, out var positions))
                return positions[0];
            return -1;
        }

        public override bool Contains(object key)
        {
            if(key == null) throw new ArgumentNullException( nameof(key));
            var tmp = ConvertToLong(key);
            return indexMap.ContainsKey(tmp);
        }
        // Lấy tất cả các vị trí của một giá trị index
        public override List<int> GetIndexPosition(object index)
        {
            var tmp = ConvertToLong(index);
            if (indexMap.ContainsKey(tmp))
            {
                return indexMap[tmp];
            }
            throw new KeyNotFoundException($"Index {index} not found.");
        }

        // Phương thức slice để lấy một phần của index
        public override IIndex Slice(int start, int end, int step)
        {
            List<object> slicedIndex = new List<object>();
            if (step == 0)
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

        public override IEnumerable<object> DistinctIndices()
        {
            return indexList.Distinct().Cast<object>();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (var item in indexList)
                yield return item;
        }
    }
}
