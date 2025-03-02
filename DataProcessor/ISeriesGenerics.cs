using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public interface ISeries<DataType>  : IEnumerable<DataType> where DataType : notnull
    {
        string? Name { get; }
        IReadOnlyList<DataType> Values { get; }
        Type dType { get; }
        bool IsReadOnly { get; }
        void Clear();
        bool Remove(DataType item);
        void Add(DataType item);
        int Count { get; }
        IList<int> Find(DataType item);
        DataType this[int index] { get; set; }

        // default IEnumerable<DataType>
        IEnumerator<DataType> IEnumerable<DataType>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        // default IEnumerable (non-generic)
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<DataType>)this).GetEnumerator();
    }
}
