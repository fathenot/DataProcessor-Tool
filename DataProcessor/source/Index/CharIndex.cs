using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Index
{
    public class CharIndex : IIndex
    {
        private readonly List<char> indexList;
        private readonly Dictionary<char, List<int>> indexMap;

        public CharIndex(List<char> indexList): base(indexList.Cast<object>().ToList())
        {
            this.indexList = indexList;
            indexMap = new Dictionary<char, List<int>>();
            // Xây dựng dictionary ánh xạ giữa index và các vị trí
            for (int i = 0; i < indexList.Count; i++)
            {
                char key = indexList[i];
                if (!indexMap.ContainsKey(key))
                {
                    indexMap[key] = new List<int>();
                }
                indexMap[key].Add(i);
            }
        }

        public override int Count => indexList.Count;
        public override IReadOnlyList<object> IndexList => indexList.Cast<object>().ToList().AsReadOnly();

        public override bool Contains(object key)
        {
            if(key is char ch)
            {
                return indexMap.ContainsKey(ch);
            }
            throw new ArgumentException($"{nameof(key)} must be char");
        }
        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            return indexMap[(char)index];
        }

        public override object GetIndex(int idx)
        {
            return indexList[idx];
        }

        public override int FirstPositionOf(object key)
        {
            if (key is char ch)
            {
                this.indexMap.TryGetValue(ch, out var index);
                if (index != null)
                    return index[0];
                return -1;
            }
            throw new ArgumentException($"{nameof(key)} must be chracter");
        }
        public override IIndex Slice(int start, int end, int step = 1)
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

            return new CharIndex(slicedIndex.Cast<char>().ToList());  // Trả về CharIndex với List<char>
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
