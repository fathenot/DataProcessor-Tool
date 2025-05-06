using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Index
{
    public class StringIndex: IIndex
    {
        private List<string> stringIndexes;
        private Dictionary<string, List<int>> stringIndexMap;

        // private method
        private void RebuildMap()
        {
            indexMap.Clear();
            for (int i = 0; i <stringIndexes.Count; i++)
            {
                string key = stringIndexes[i];
                if (!indexMap.ContainsKey(key))
                    indexMap[key] = new List<int>();
                indexMap[key].Add(i);
            }
        }

        public StringIndex(List<string> stringIndexes) : base(stringIndexes.Cast<object>().ToList())
        {
            this.stringIndexes = stringIndexes;
            stringIndexMap = new Dictionary<string, List<int>>();
            for (int i = 0; i < stringIndexes.Count; i++)
            {
                if (!stringIndexMap.ContainsKey(stringIndexes[i]))
                {
                    stringIndexMap[stringIndexes[i]] = new List<int>();
                }
                stringIndexMap[stringIndexes[i]].Add(i);
            }
        }

        public void Add(string index)
        {
            stringIndexes.Add(index);
            base.Add(index);
            if (!stringIndexMap.ContainsKey(index))
            {
                stringIndexMap[index] = new List<int>();
            }
            stringIndexMap[index].Add(Count - 1);
        }

        public override IIndex Slice(int start, int end, int step = 1)
        {
            List<string> slicedIndex = new List<string>();

            if (step == 0)
            {
                throw new ArgumentException($"step must not be 0");
            }
            else if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    slicedIndex.Add(stringIndexes[i]);
                }
            }
            else
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedIndex.Add(stringIndexes[i]);
                }
            }
            return new StringIndex(slicedIndex);
        }

        protected override void Drop(object key)
        {
            if (key is not string stringKey || !indexMap.ContainsKey(stringKey))
                return;

            var positions = indexMap[stringKey];
            foreach (var pos in positions.OrderByDescending(p => p))
            {
                indexList.RemoveAt(pos);
            }

            indexMap.Remove(stringKey);

            // Cập nhật lại indexMap vì vị trí các phần tử phía sau đã thay đổi
            RebuildMap();
        }
    }
}
