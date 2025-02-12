using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;
using System.Buffers;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data;
using System.ComponentModel.DataAnnotations;
namespace DataProcessor
{

    public class DataFrame
    {
        private List<Series> table;
        private List<String> columns;
        private int numRows;
        public DataFrame(List<Series> table)
        {
            if(table == null) throw new ArgumentNullException("table must not be null");
            this.table = new List<Series>(table);
            this.columns = new List<String>();
            numRows = 0;
            foreach(Series s in table)
            {
                this.table.Add(new Series(s));
                this.columns.Add(s.Name);
                numRows = Math.Max(0, s.Count);
            }
        }

        public DataFrame(Dictionary<string, IList<object>> items)// string for column name, List<string> for values in a column
        {
            if (items == null)
            {
                throw new ArgumentNullException($"{nameof(items)} must not be null");
            }
            table = new List<Series>();
            columns = new List<String>();
            foreach (var columnName in items.Keys)
            {
                Series column = new Series(columnName, items[columnName]);
                table.Add(column);
                columns.Add(columnName);
            }
        }
        public DataFrame(DataFrame other)
        {
            this.table = new List<Series>(other.table);
            this.columns = new List<String>(other.Columns);
        }
        public List<String> Columns => columns;
        public void Describe() { throw new NotImplementedException("this method is under construction"); }
        public DataFrame Head(int numberRows = 5)
        {
            if(numberRows < 0) { throw new ArgumentOutOfRangeException("number of rows must not be negative"); }
            List<Series> list = new List<Series>();
            foreach (Series s in this.table)
            {
                list.Add(new Series(s.Name, s.Values is List<object> listRef ? listRef.GetRange(0, Math.Min(numberRows, listRef.Count)) : s.Values.Take(numberRows).ToList()));
            }
            return new DataFrame(list);
        }
        public DataFrame Tail(int numberRows = 5)
        {
            if (numberRows < 0) { throw new ArgumentOutOfRangeException("number of rows must not be negative"); }
            List<Series> list = new List<Series>();
            foreach (Series s in this.table)
            {
                list.Add(new Series(s.Name, s.Values is List<object> listRef ? listRef.GetRange(table[0].Count - numberRows, numberRows) : s.Values.Take(numberRows).ToList()));
            }
            return new DataFrame(list);

        }
    }
}
