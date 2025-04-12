using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        public string? Name { get { return this.seriesName; } }
        public IReadOnlyList<object?> Values => values == null ? throw new Exception("values list is null") : values as IReadOnlyList<object?> ?? values.ToList();
        public int Count => values == null ? 0 : values.Count;
        public bool IsReadOnly { get { return false; } }
        public Type DataType
        {
            get => dataType;
            private set => dataType = value;
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
        public List<object> Index
        {
            get
            {
                return this.index;
            }
            set
            {
                List<object> newIndex = value;
                index.Clear();
                if (newIndex != null)
                {
                    this.index = new List<object>(newIndex);
                    for (int i = 0; i < index.Count; i++)
                    {
                        // Add index to map (if not exists, create new entry)
                        if (!indexMap.TryGetValue(index[i], out var list))
                        {
                            list = new List<int>();
                            indexMap[index[i]] = list;
                        }
                        list.Add(i);
                    }

                    // Ensure values list is extended to match the index size
                    while (values.Count < index.Count)
                    {
                        values.Add(null);
                    }
                }
                else
                {
                    this.index = new List<object>(Enumerable.Range(0, values.Count).Cast<object>());
                    for (int i = 0; i < values.Count; i++)
                    {
                        indexMap[i] = new List<int> { i };
                    }
                }
                synchronizer.Notify();
            }
        }
    }
}
