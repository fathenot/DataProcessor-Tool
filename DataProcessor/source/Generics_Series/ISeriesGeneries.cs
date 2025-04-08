using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Generics_Series
{
    public interface ISeries<DataType> : IEnumerable<DataType> where DataType : notnull
    {
        string? Name { get; }
        IReadOnlyList<DataType> Values { get; }
        Type dType { get; }
        bool IsReadOnly { get; }
        void Clear();
        int Count { get; }
        IList<int> Find(DataType item);
    }
}
