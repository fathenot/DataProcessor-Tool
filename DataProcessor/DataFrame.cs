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
using System.Globalization;
using System.Collections.Generic;

namespace DataProcessor
{
    public interface IDataFrame
    {
        public List<string> Columns { get; }
    }
    public class DataFrame : IDataFrame
    {
        private List<Series> table;
        private List<string> columns;
        private int numRows;
        
        //constructor
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
                numRows = Math.Max(this.numRows, s.Count);
            }
            foreach(Series series in table)
            {
                for(int i = 1; i <= this.numRows - series.Count; i++)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    series.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
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
            Console.WriteLine($"Number of rows {this.numRows}");
            Console.WriteLine($"Number of columns {columns.Count}");
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
            
            int[] dataIndex = Enumerable.Range(0, this.numRows).ToArray();// this array store the row index of origin data that in the right order
            int columnPos = this.columns.IndexOf(column);
            Comparer<object> comparer = Comparer<object>.Default;
            // Sort dataIndex based on column values
            Array.Sort(dataIndex, (i, j) =>
            {
                object a = data.Values[i];
                object b = data.Values[j];
                return (a, b) switch
                {
                    (null, null) => 0,
                    (null, _) => -1,
                    (_, null) => 1,
                    _ => comparer.Compare(a, b)
                };
            });
            if (!ascending)
            {
                Array.Reverse(dataIndex);
            }
            // set new values
            for(int colIndex = 0; colIndex < this.columns.Count; colIndex++)
            {
                Series newSeries= new Series(getColumn(columns[colIndex]));
                IList<object> values = newSeries.Values;
                newSeries.Clear();
                for (int rowIndex = 0; rowIndex < dataIndex.Length; rowIndex++)
                {
                    newSeries.Add(values[dataIndex[rowIndex]]);
                }
                // replace old column to new column
                table[colIndex] = newSeries;
            }

        }
    }
}
