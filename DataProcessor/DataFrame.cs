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
using System.Reflection.Metadata.Ecma335;

namespace DataProcessor
{

    public class DataFrame
    {
        private List<Series> table;
        private List<string> columns;
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
        // properties
        public List<string> Columns => columns;

        // method
        public void Describe() 
        { 
            throw new NotImplementedException("this method is under construction");
        }

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

        public Series getColumn(string column)
        {
            int colIndex = this.columns.IndexOf(column);
            if (colIndex == -1)
            {
                throw new ArgumentException($" Column {column} is not exist");
            }
            return table[colIndex];
        }

        public void Sort(string column, bool ascending = true) // sort by column
        { 
            Series data = this.getColumn(column);
            
            int[] dataIndex = Enumerable.Range(0, this.numRows).ToArray();// this array store the index of origin data that in the right order
            int columnPos = this.columns.IndexOf(column);
            Array.Sort(dataIndex, (i, j) =>
            {
                object a = data.Values[i];
                object b = data.Values[j];
                if (a == null && b != null)
                {
                    return -1;
                }
                if (a == null && b == null)
                {
                    return 0;
                }
                if (a != null && b == null)
                {
                    return 1;
                }
                return Comparer<object>.Default.Compare(a, b);
            });
            if (!ascending)
            {
                dataIndex = dataIndex.Reverse().ToArray();
            }

            for(int colIndex = 0; colIndex < column.Length; colIndex++)
            {
                Series newSeries= new Series(getColumn(columns[colIndex]));
                newSeries.Clear();
                for (int rowIndex = 0; rowIndex < dataIndex.Length; rowIndex++)
                {
                    newSeries.Add(data[rowIndex]);
                }
                // replace old column to new column
                table[colIndex] = newSeries;
            }

        }
    }
}
