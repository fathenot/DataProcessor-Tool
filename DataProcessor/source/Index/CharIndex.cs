using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Index
{
    public class CharIndex : IIndex
    {
        private new readonly List<char> indexList;
        private new readonly Dictionary<char, List<int>> indexMap;

        private void RebuildMap()
        {
            indexMap.Clear();
            for (int i = 0; i < indexList.Count; i++)
            {
                char key = indexList[i];
                if (!indexMap.ContainsKey(key))
                    indexMap[key] = new List<int>();
                indexMap[key].Add(i);
            }
        }
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

        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            return indexMap[(char)index];
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

        protected override void Add(object key)
        {
            if (key is char charKey)
            {
                indexList.Add(charKey);
                if (!indexMap.ContainsKey(charKey))
                {
                    indexMap[charKey] = new List<int>();
                }
                indexMap[charKey].Add(Count - 1);
            }
        }

        protected override void Drop(object key)
        {
            if(key is not char charKey || !indexMap.ContainsKey(charKey))
                return;

            var positions = indexMap[charKey];
            foreach (var pos in positions.OrderByDescending(p => p))
            {
                indexList.RemoveAt(pos);
            }

            indexMap.Remove(charKey);

            // Cập nhật lại indexMap vì vị trí các phần tử phía sau đã thay đổi
            RebuildMap();
        }
    }
}
