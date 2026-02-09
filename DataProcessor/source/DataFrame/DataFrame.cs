using System;
using System.Linq;
using System.Text;
using System.IO;
using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.IndexTypes;
using System.Runtime.CompilerServices;

namespace DataProcessor.source.DataFrame
{
    public sealed partial class DataFrame
    {
        private List<Series> frame;
        private ColumnRegistry columnRegistry;
        private DataIndex index;
        private Schema schema;
        internal DataFrame(List<string> columnName, List<Series> series, DataIndex? index = null)
        {
            this.columnRegistry = new ColumnRegistry(columnName);
            this.frame = new List<Series> (series);
            int rowCount = series.Count == 0 ? 0 : series[0].Count;

            this.index = index == null
                ? new RangeIndex(0, rowCount)
                : index.Clone();

            // create schema
            List<Type> types = new List<Type> ();
            foreach (var serie in series)
            {
                types.Add(serie.DataType);
            }
            var schema = new List<(string, Type)> ();
            foreach (var t in columnName.Zip(types))
            {
                schema.Add(t);
            }
            this.schema = new Schema(schema);
        }

    }
}

