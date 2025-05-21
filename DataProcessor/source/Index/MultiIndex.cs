namespace DataProcessor.source.Index
{
    public readonly struct MultiKey : IEquatable<MultiKey>
    {
        private readonly object[] _values;

        public MultiKey(params object[] values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }


        public MultiKey(MultiKey other)
        {
            _values = new object[other.Values.Length];
            for (int i = 0; i < other.Values.Length; i++)
            {
                _values[i] = other.Values[i];
            }
        }
        // Sửa lại Equals để kiểm tra null đúng cách và so sánh theo giá trị từng phần tử
        public bool Equals(MultiKey other)
        {
            if (_values.Length != other._values.Length) return false;

            for (int i = 0; i < _values.Length; i++)
            {
                // Kiểm tra giá trị null để tránh NullReferenceException
                if (_values[i] == null && other._values[i] != null || _values[i] != null && !_values[i].Equals(other._values[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj)
            => obj is MultiKey other && Equals(other);

        // Cập nhật GetHashCode để xử lý null và đảm bảo tính đồng nhất với Equals
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var val in _values)
            {
                // Null kiểm tra trước khi gọi GetHashCode
                hash = hash * 31 + (val?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public object this[int index] => _values[index];

        public int Length => _values.Length;

        public override string ToString() => $"({string.Join(", ", _values)})";

        public object[] Values => _values;
    }

    public class MultiIndex : IIndex
    {
        private List<MultiKey> indexList;
        private Dictionary<MultiKey, List<int>> indexMap;


        public MultiIndex(List<object[]> indexList) : base()
        {
            this.indexList = new List<MultiKey>(indexList.Count);
            indexMap = new Dictionary<MultiKey, List<int>>();

            // Xây dựng dictionary ánh xạ giữa index và các vị trí
            for (int i = 0; i < indexList.Count; i++)
            {
                var key = new MultiKey(indexList[i]);
                this.indexList[i] = key;

                if (!indexMap.ContainsKey(key))
                {
                    indexMap[key] = new List<int>();
                }
                indexMap[key].Add(i);
            }
        }

        public MultiIndex(List<MultiKey> indexList)
        {
            this.indexList = indexList;
            this.indexMap = new Dictionary<MultiKey, List<int>>();
            for (int i = 0; i < indexList.Count; i++)
            {
                var key = indexList[i];
                if (!indexMap.ContainsKey(key))
                {
                    indexMap[key] = new List<int>();
                }
                indexMap[key].Add(i);
            }
        }

        // properties
        public override int Count => indexList.Count;

        public override IReadOnlyList<object> IndexList => this.indexList.Cast<object>().ToList().AsReadOnly();

        // methods
        public override MultiIndex Slice(int start, int end, int step = 1)
        {
            // Cắt phần tử từ indexList theo start, end, step
            var slicedList = indexList.GetRange(start, end - start)
                                      .Where((item, index) => index % step == 0)
                                      .ToList();

            return new MultiIndex(slicedList);
        }
        public MultiIndex SliceLevel(int level, object key)
        {
            // Lọc các phần tử có giá trị tại level == key
            var filteredList = indexList.Where(item => EqualityComparer<object>.Default.Equals(item[level], key))
                                        .ToList();

            return new MultiIndex(filteredList);
        }

        public bool Contains(object[] key)
        {
            var multiKey = new MultiKey(key);
            return indexMap.ContainsKey(multiKey);
        }

        public override bool Contains(object key)
        {
            if (key is MultiKey multiKey)
            {
                return indexMap.ContainsKey(multiKey);
            }

            if(key is object[] arr)
            {
                return this.Contains(arr);
            }

            throw new ArgumentException($"{nameof(key)} must be object array or multikey");
        }
        public IReadOnlyList<int> GetIndexPosition(object[] key)
        {
            var multiKey = new MultiKey(key);
            if (indexMap.ContainsKey(multiKey))
            {
                return indexMap[multiKey];
            }
            throw new KeyNotFoundException($"Key {string.Join(", ", key)} not found.");
        }

        public override IList<int> GetIndexPosition(object index)
        {
            if (index is object[] arr)
            {
                return new List<int> (GetIndexPosition(arr));
            }
            if (index is MultiKey multiKey)
            {
                return indexMap[multiKey];
            }
            throw new ArgumentException($"index {nameof(index)} must be array of object of multikey");
        }

        public override int FirstPositionOf(object key)
        {
            if(key is MultiKey multiKey)
            {
                return indexList.IndexOf(multiKey);
            }
            if( key is object[] arr)
            {
                var tmp = new MultiKey(arr);
                return indexList.IndexOf(tmp);
            }
            throw new ArgumentException($"{nameof(key)} must be object array or multikey");
        }

        // Phương thức để lấy index của tuple tại vị trí idx
        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        // Phương thức để lấy các distinct index
        public override IEnumerable<object> DistinctIndices()
        {
            return indexList.Distinct().Cast<object>();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            for (int i = 0; i < indexList.Count; i++)
            {
                yield return indexList[i];
            }
        }
    }
}
