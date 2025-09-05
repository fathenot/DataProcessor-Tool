using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.GenericsSeries
{
    public interface ISeries<DataType> : IEnumerable<DataType>
    {
        string? Name { get; }
        IReadOnlyList<DataType> Values { get; }
        Type DType { get; }
        bool IsReadOnly { get; }
        int Count { get; }
    }
}
