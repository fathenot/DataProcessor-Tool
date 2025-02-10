using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;
using System.Buffers;
using System.Data.Common;
using System.Data.SqlTypes;
namespace DataProcessor
{

    public class DataFrame
    {
        private List<Series> table;

        public DataFrame(List<Series> table)
        {
            this.table = table;
        }

        public DataFrame(Dictionary<string, IList<object>> items)// string for column name, List<string> for value in a column
        {
            if (items == null)
            {
                throw new ArgumentNullException($"{nameof(items)} must not be null");
            }
            table = new List<Series>();
            foreach (var columnName in items.Keys)
            {
                Series column = new Series(columnName, items[columnName]);
                table.Add(column);
            }
        }
    }
}
