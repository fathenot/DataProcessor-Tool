namespace DataProcessor.source.Index
{
    public class DateTimeIndex : IIndex
    {
        private readonly List<DateTime> dateTimes;
        private new readonly Dictionary<DateTime, List<int>> indexMap;

        // private methods
        private void RebuildMap()
        {
            indexMap.Clear();
            for (int i = 0; i < indexList.Count; i++)
            {
                DateTime key = dateTimes[i];
                if (!indexMap.ContainsKey(key))
                    indexMap[key] = new List<int>();
                indexMap[key].Add(i);
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

        public override IReadOnlyList<int> GetIndexPosition(object datetime)
        {
            if (datetime is DateTime time && indexMap.ContainsKey(time))
            {
                return indexMap[time];
            }
            throw new KeyNotFoundException($"time {datetime} not found");
        }

        public override object GetIndex(int idx)
        {
            return dateTimes[idx];
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
        protected override void Add(object key)
        {
            if (key is DateTime time)
            {
                indexList.Add(key);
                if (!indexMap.ContainsKey(time))
                {
                    indexMap[time] = new List<int>();
                }
                indexMap[time].Add(Count - 1);
                base.Add(key);
                return;
            }
            throw new InvalidDataException($"{nameof(key)} must be datetime");
        }
        protected override void Drop(object key)
        {
            if (key is DateTime time)
            {
                indexList.Remove(key);
                RebuildMap();
            }
            base.Drop(key);
        }
    }
}
