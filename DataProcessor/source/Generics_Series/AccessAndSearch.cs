using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Generics_Series
{
    public partial class Series<DataType>
    {
        // accessing the series
        public IList<DataType> GetItem(int indexFrom, int indexTo, int step = 1)
        {
            IList<DataType> result = new List<DataType>();

            // check valid argument
            if (indexFrom < 0 || indexTo < 0 || indexFrom >= values.Count || indexTo >= values.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }

            if (step < 0)
            {
                if (indexFrom < indexTo)
                {
                    throw new ArgumentException("Index from must be greater than index to when step < 0");
                }
            }

            if (step > 0 && indexFrom > indexTo)
            {
                throw new ArgumentException("Index from must be smaller than index to when step > 0");
            }

            if (step > values.Count)
            {
                throw new ArgumentOutOfRangeException("Step greater than size of series");
            }

            if (step == 0)
            {
                throw new ArgumentException("Step must not be zero");
            }

            //main logic of the method
            if (indexTo == indexFrom)
            {
                return result;  // Trả về danh sách rỗng theo phong cách Python
            }
            if (indexTo == indexFrom)
            {
                return result;  // Trả về danh sách rỗng theo phong cách Python
            }
            if (step > 0)
            {
                for (int i = indexFrom; i <= indexTo; i += step)
                {
                    result.Add(values[i]);
                }
            }
            else
            {
                for (int i = indexFrom; i >= indexTo; i += step)
                {
                    result.Add(values[i]);
                }
            }
            return result;
        }

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
            IList<int> Indexes = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<DataType>.Default.Equals(values[i], item))
                    Indexes.Add(i);
            }
            return Indexes;
        }
    }
}
