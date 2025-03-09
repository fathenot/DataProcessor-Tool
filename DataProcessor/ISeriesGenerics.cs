using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public interface ISeries<DataType> where DataType : notnull
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
    }
}
