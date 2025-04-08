using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Non_Generics_Series
{
    public partial class Series
    {
        public string? Name { get { return this.name; } }
        public IReadOnlyList<object?> Values => values == null ? throw new Exception("values list is null") : values as IReadOnlyList<object?> ?? values.ToList();
        public int Count => values == null ? 0 : values.Count;
        public bool IsReadOnly { get { return false; } }
        public Type dType
        {
            get => dtype;
            private set => dtype = value;
        }
        public List<object?> this[object index]
        {
            get
            {
                if (!this.Contains(index))
                {
                    throw new ArgumentException("index not found", nameof(index));
                }
                List<object?> res = new List<object?>();
                foreach (int i in this.indexMap[index])
                {
                    res.Add(this.values[i]);
                }
                return res;
            }
        }
        public IReadOnlyList<object> Index => this.index;
    }
}
