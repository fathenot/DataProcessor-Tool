using System.Collections.Generic;

namespace DataProcessor.source.IndexTypes
{
    public class DateTimeIndex : DataIndex
    {
        private readonly List<DateTime> dateTimes;
        private readonly Dictionary<DateTime, List<int>> indexMap;

        // private methods

        private static DateTime ConvertToDateTime(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            try
            {
                return Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {

                throw new ArgumentException($"Invalid index: cannot convert {value} to long.", ex);
            }
        }

        public DateTimeIndex(List<DateTime> times) : base(times.Cast<object>().ToList())
        {
            this.dateTimes = times;
            indexMap = new Dictionary<DateTime, List<int>>();
            for (int i = 0; i < times.Count; i++)
            {
                if (!indexMap.ContainsKey(times[i]))
                {
                    indexMap[times[i]] = new List<int>();
                }
                indexMap[times[i]].Add(i);
            }

        }

        public override int Count => dateTimes.Count;
        public override IReadOnlyList<object> IndexList => dateTimes.Cast<object>().ToList().AsReadOnly();
        public override IReadOnlyList<int> GetIndexPosition(object datetime)
        {
            if (datetime is DateTime time && indexMap.ContainsKey(time))
            {
                return indexMap[time];
            }
            throw new KeyNotFoundException($"time {datetime} not found");
        }

        public override bool Contains(object key)
        {
            var tmp = ConvertToDateTime(key);
            return indexMap.ContainsKey(tmp);
        }
        public override object GetIndex(int idx)
        {
            if (idx < 0 || idx >= dateTimes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(idx), "Index is out of range.");
            }
            return dateTimes[idx];
        }

        public override int FirstPositionOf(object key)
        {
            var tmp = ConvertToDateTime(key);
            if (indexMap.ContainsKey(tmp))
                return indexMap[tmp][0];
            return -1;
        }
        public override DataIndex Slice(int start, int end, int step = 1)
        {
            List<DateTime> slicedIndex = new List<DateTime>();
            // validate parameters
            if (step == 0)
            {
                throw new ArgumentException($"step must not be 0");
            }
            if (start < 0 || end < 0 || start >= dateTimes.Count || end >= dateTimes.Count)
            {
                throw new ArgumentOutOfRangeException("start or end index is out of range.");
            }

            else if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    slicedIndex.Add(dateTimes[i]);
                }
            }
            else
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedIndex.Add(dateTimes[i]);
                }
            }
            return new DateTimeIndex(slicedIndex);
        }

        public override DataIndex TakeKeys(List<object> indexList)
        {
            List<DateTime> slicedIndex = new List<DateTime>();
            foreach (var item in indexList)
            {
                if (item is DateTime dateTime && indexMap.ContainsKey(dateTime))
                {
                    slicedIndex.Add(dateTime);
                }
                else
                {
                    throw new ArgumentException($"Invalid item {item} in index list.");
                }
            }
            return new DateTimeIndex(slicedIndex);
        }

        public override IEnumerable<object> DistinctIndices()
        {
            return dateTimes.Distinct().Cast<object>();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (var item in dateTimes)
                yield return item;
        }

        public override object this[int index]
        {
            protected set
            {
                if (index < 0 || index >= dateTimes.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
               DateTime oldValue = dateTimes[index];
                dateTimes[index] = (DateTime)value;

                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (!indexMap[oldValue].Any())
                    {
                        indexMap.Remove(oldValue);
                    }
                }

                if (!indexMap.ContainsKey((DateTime)value))
                {
                    indexMap[(DateTime)value] = new List<int>();
                }
                indexMap[(DateTime)value].Add(index);
            }
        }

        public override DataIndex Clone()
        {
            return new DateTimeIndex(dateTimes);
        }
    }
}
