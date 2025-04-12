using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType>
    {
        // accessing the series
        public Series<DataType> Head(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<DataType> items = new List<DataType>();
            for (int i = 0; i < count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series<DataType>(this.name, items);
        }

        public Series<DataType> Tail(int count)
        {
            if (count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            List<DataType> items = new List<DataType>();
            for (int i = this.Count - count; i < this.Count; i++)
            {
                items.Add(this.values[i]);
            }
            return new Series<DataType>(name, items);
        }

        // searching and filter
        public IList<DataType> Filter(Func<DataType, bool> filter)
        {
            return values.AsEnumerable().Where(filter).ToList();
        }

        public bool Contains(DataType item)
        {
            return values.Contains(item);
        }

        public IList<int> Find(DataType item)
        {
            List<int> Indexes = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<DataType>.Default.Equals(values[i], item))
                    Indexes.Add(i);
            }
            return Indexes;
        }
    }
}
