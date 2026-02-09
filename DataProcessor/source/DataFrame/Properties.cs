using DataProcessor.source.IndexTypes;
using DataProcessor.source.NonGenericsSeries;

namespace DataProcessor.source.DataFrame
{
    public sealed partial class DataFrame
    {
        public (int row, int column) Shape { get => (this.frame[0].Count, this.columnRegistry.GetNumColumns()); }

        public int RowCount { get => this.frame[0].Count; }

        public int ColumnCount { get => this.columnRegistry.GetNumColumns(); }

        public IReadOnlyList<string> Columns { get => this.columnRegistry.Columns; }

        public DataIndex Index { get => index.Clone(); }

        public IReadOnlyList<Type> DTypes
        {
            get
            {
                List<Type> types = new List<Type>();
                foreach (Series series in this.frame)
                {
                    types.Add(series.DataType);
                }
                return types;
            }
        }

        public bool IsEmpty { get => RowCount == 0 || ColumnCount == 0; }

        internal IReadOnlyList<Series> Series { get => frame; }

    }
}
