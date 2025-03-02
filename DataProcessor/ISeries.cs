using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    public interface ISeries
    {
        public string? Name { get; }
        public IReadOnlyList<object?> Values { get; }
        public Type dType { get; }
        public bool IsReadOnly { get; }
        public void Clear();
        public bool Remove(object item);
        public void Add(object item);
        public bool IsValidType(object? value);
        public int Count { get; }
        public ISeries Clone();
        public ISeries AsType(Type NewType, bool ForceCast = false);
        public IList<int> Find(object? item);
        public object? this[int index] { get; set; }
        public ISeries View(ValueTuple<int, int, int> slice);
    }
}
