using System.Collections;

namespace DataProcessor.source.GenericsSeries
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
                // Check if index is valid
                var positions = this.index.GetIndexPosition(index);
                List<DataType> result = new List<DataType>();
                Converter<object, DataType> converter = (x) => (DataType)x;
                foreach (var item in positions)
                {
                    result.Add(converter(values.GetValue(item)));
                }
                return result;
            }
        }
        public Type DType => typeof(DataType);
        public IReadOnlyList<DataType> Values => values.Cast<DataType>().ToList();
        public IReadOnlyList<object> Index => this.index.IndexList;

        // iterator
        public IEnumerator<DataType> GetEnumerator()
        {
            return values.Cast<DataType>().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
