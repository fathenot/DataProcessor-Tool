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

        /// <summary>
        /// Initializes a new instance of the <see cref="CharIndex"/> class, which maps characters to their positions in
        /// the provided list.
        /// </summary>
        /// <remarks>This constructor builds an internal dictionary that maps each unique character in the
        /// provided list to a list of its indices. The mapping allows for efficient lookups of character positions
        /// within the list.</remarks>
        /// <param name="indexList">A list of characters to be indexed. Each character in the list will be mapped to its corresponding
        /// positions.</param>
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
            //validate parameters
            if (step == 0)
            {
                throw new ArgumentException("step must not be 0");
            }
            if(start < 0 || end < 0 || start >= indexList.Count || end >= indexList.Count)
            {
                throw new ArgumentOutOfRangeException("start or end is out of range of the index list.");
            }
            // Kiểm tra điều kiện bước nhảy âm
            if (step > 0)
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

            return new CharIndex(slicedIndex.Cast<char>().ToList());  // Trả về CharIndex với List<char>
        }

        public override IIndex Slice(List<object> indexList)
        {
            var slicedIndex = new List<char>();
            foreach (var item in indexList)
            {
                if (item is char chn && indexMap.ContainsKey(chn))
                {
                    foreach (var index in indexMap[chn])
                    {
                        slicedIndex.Add(chn);
                    }
                }
                else
                {
                    throw new ArgumentException($"All items in indexList must be of type char or in the index list");
                }
            }

            return new CharIndex(slicedIndex);
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

        public override object this[int index]
        {
            protected set
            {
                if (index < 0 || index >= indexList.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                char oldValue = indexList[index];
                indexList[index] = (char)value;
                
                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (!indexMap[oldValue].Any())
                    {
                        indexMap.Remove(oldValue);
                    }
                }
                
                if (!indexMap.ContainsKey((char)value))
                {
                    indexMap[(char)value] = new List<int>();
                }
                indexMap[(char)value].Add(index);
            }
        }

        public override IIndex Clone()
        {
            return new CharIndex(indexList);
        }
    }
}
