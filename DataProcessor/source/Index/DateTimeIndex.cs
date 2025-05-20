namespace DataProcessor.source.Index
{
    public class DateTimeIndex : IIndex
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
            catch (Exception ex )
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
            return dateTimes[idx];
        }

        public override int FirstPositionOf(object key)
        {
            var tmp = ConvertToDateTime(key);
            if (indexMap.ContainsKey(tmp))
                return indexMap[tmp][0];
            return -1;
        }
        public override IIndex Slice(int start, int end, int step = 1)
        {
            List<DateTime> slicedIndex = new List<DateTime>();
            if (step == 0)
            {
                throw new ArgumentException($"step must not be 0");
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

        public override IEnumerable<object> DistinctIndices()
        {
            return dateTimes.Distinct().Cast<object>();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            foreach (var item in dateTimes)
                yield return item;
        }
    }
}
