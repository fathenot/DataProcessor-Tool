using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Non_Generics_Series
{
    public partial class Series
    {
        // access the series
        public ISeries Head(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<object?> items = new List<object?>();
            for (int i = 0; i < count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series(items, this.name);
        }

        public ISeries Tail(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<object?> items = new List<object?>();
            for (int i = this.Count - count; i < this.Count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series(items, name: name);
        }

        public View GetView((object start, object end, int step) slices)
        {
            // check valid start, end
            if (!indexMap.ContainsKey(slices.start))
            {
                throw new ArgumentException($"start index: {slices.start} not exist");
            }
            if (!indexMap.ContainsKey(slices.start))
            {
                throw new ArgumentException($"End index: {slices.end} not exist");
            }
            return new View(this, slices);
        }

        public View GetView(List<object> slice)
        {
            // check valid argument
            var invalidIndex = slice.Where(v => !this.index.Contains(v)).ToList();
            if(invalidIndex.Count != 0)
            {
                throw new ArgumentException($"slice contains index(es) that does not exist in the index of the series{string.Join(", ", invalidIndex.Select(x => x?.ToString()))}");
            }
            
            return new View(this, slice);
        }

        // searching and filter
        public IList<object?> Filter(Func<object?, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }

        public List<int> Find(object? item) // find all occurance indexes of item
        {
            List<int> Indexes = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (Equals(this[i], item)) { Indexes.Add(i); }
            }
            return Indexes;
        }

        public bool Contains(object? item)
        {
            return values == null ? false : values.Contains(item);
        }
    }
}
