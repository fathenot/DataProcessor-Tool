using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Generics_Series
{
    public partial class Series<DataType>
    {
        // utility
        public string? Name { get { return this.name; } }
        public int Count => values.Count;
        public bool IsReadOnly => false;
        public List<DataType> this[object index]
        {
            get
            {
                if (!this.indexMap.TryGetValue(index, out List<int>? indexNum))
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
                List<DataType> res = new List<DataType>();
                foreach (var idx in indexNum)
                {
                    res.Add(this.values[idx]);
                }
                return res;
            }
        }
        public Type dType => typeof(DataType);
        public IReadOnlyList<DataType> Values => values.AsReadOnly();
        public IReadOnlyList<object> Index => this.index;

        // iterator
        public IEnumerator<DataType> GetEnumerator()
        {
            return values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
