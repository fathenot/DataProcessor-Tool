using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public interface ISeries: IEnumerable<object?>
    {
        public string? Name { get; }
        public IReadOnlyList<object?> Values { get; }
        public Type dType { get; }
        public bool IsReadOnly { get; }
        public void Clear();
        public int Count { get; }
        public ISeries AsType(Type NewType, bool ForceCast = false);
        public List<int> Find(object? item);
    }
}
