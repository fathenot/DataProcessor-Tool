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

        private static string NormalizeUnicode(string s)
    => s.Normalize(NormalizationForm.FormC); // chuẩn hóa tổ hợp glyph


        public StringIndex(List<string> stringIndexes) : base(stringIndexes.Cast<object>().ToList())
        {
            this.stringIndexes = new List<string>(stringIndexes.Count);
            stringIndexMap = new Dictionary<string, List<int>>();
            for (int i = 0; i < stringIndexes.Count; i++)
            {
                var normalizedString = NormalizeUnicode(stringIndexes[i]);
                stringIndexes[i] = normalizedString;
                if (!stringIndexMap.ContainsKey(normalizedString))
                {
                    stringIndexMap[normalizedString] = new List<int>();
                }
                stringIndexMap[normalizedString].Add(i);
            }
        }

        public override int Count => stringIndexes.Count;

        public override IReadOnlyList<object> IndexList => stringIndexes.Cast<object>().ToList().AsReadOnly();
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

        public override bool Contains(object key)
        {
            if(key is string strKey)
            {
                var tmp = NormalizeUnicode(strKey);
                return stringIndexMap.ContainsKey(tmp);
            }
            throw new ArgumentException($"{nameof(key)} must be string.");
        }

        public override int FirstPositionOf(object key)
        {
            if (key is string strKey)
            {
                var tmp = NormalizeUnicode(strKey);
                return stringIndexMap[tmp][0];
            }
            throw new ArgumentException($"{nameof(key)} must be string.");
        }
        public override object GetIndex(int idx)
        {
            return stringIndexes[idx];
        }

        public override IList<int> GetIndexPosition(object index)
        {
            return new List<int> (stringIndexMap[(string)index]);
        }

        public override IEnumerable<object> DistinctIndices()
        {
           return stringIndexes.Distinct();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (string index in stringIndexes)
            {
                yield return index;
            }
        }
    }
}
