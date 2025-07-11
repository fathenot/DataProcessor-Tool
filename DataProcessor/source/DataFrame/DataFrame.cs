using System;
using System.Linq;
using System.Text;
using System.IO;
using DataProcessor.source.NonGenericsSeries;

namespace DataProcessor.source.DataFrame
{
    class DataFrame
    {
        private List<string?> columns;
        private List<object> index;
        private List<Series> frame;
        private Dictionary<object, List<int>> IndexToLineNums;
        bool defaultIndex;
        // Constructor
        public DataFrame()
        {
            columns = new List<string?>();
            index = new List<object>();
            frame = new List<Series>();
            IndexToLineNums = new Dictionary<object, List<int>>();
            defaultIndex = true;
        }
        public DataFrame(DataFrame other)
        {
            columns = new List<string?>(other.columns);
            index = new List<object>(other.index);
            frame = new List<Series>();
            frame.AddRange(other.frame);
            IndexToLineNums = new Dictionary<object, List<int>>();
            foreach (var idx in other.index)
            {
                IndexToLineNums[idx] = new List<int>(other.IndexToLineNums[idx]);
            }
        }
        public DataFrame(Dictionary<string, List<object?>> NameAndValues, List<object>? index = null)
        {
            columns = NameAndValues.Keys.ToList(); // khởi tạo columns

            // khởi tạo dãy các series
            List<List<object?>> rows = new List<List<object?>>();
            foreach (var key in columns)
            {
                rows.Add(new List<object?>(NameAndValues[key]));
            }
            Supporter.ScaleLength(rows);
            List<Series> series = new List<Series>();
            foreach (var row in rows)
            {
                series.Add(new Series(row, index: index));
            }

            // khởi tạo index và ánh xạ sang vị trí
            if (index == null)
            {
                defaultIndex = true;
                IndexToLineNums = new Dictionary<object, List<int>>();
                this.index = Enumerable.Range(0, series[0]!.Count).Cast<object>().ToList();
                for (int i = 0; i < series[0].Count; i++)
                {
                    IndexToLineNums[i] = new List<int> { i };
                }
            }
            else
            {
                defaultIndex = false;
                if (index.Count != rows.Count)
                {
                    throw new ArgumentException($"Index size must equal the number of rows of series; Number of rows is {Shape.Rows} ");
                }
                this.index = new List<object>(index);
                IndexToLineNums = new Dictionary<object, List<int>>();
                for (int i = 0; i < index.Count; i++)
                {
                    var key = index[i];
                    if (!IndexToLineNums.ContainsKey(key))
                    {
                        IndexToLineNums[key] = new List<int>();
                    }
                    IndexToLineNums[key].Add(i);
                }
            }
            // hết phần khởi tạo index và ánh xạ

        }
        public DataFrame(List<Series> frame, List<string?>? columns = null, List<object>? index = null)
        {
            // init attributes
            columns = new List<string?>();
            this.index = new List<object>();
            this.frame = new List<Series>();

            // configure the frame begin
          


        }
        //Properties
        public List<string?> Columns { get { return columns; } }
        public List<object> Index { get { return index; } }
        public Series this[string ColumnName, int nth = 1]
        {
            get
            {
                int count = 0;
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i] == ColumnName)
                    {
                        count++;
                        if (count == nth)
                        {
                            return new Series(frame[i]); // Lấy ngay khi tìm đủ nth
                        }
                    }
                }
                if (count == 0)
                {
                    throw new ArgumentException($"Column '{ColumnName}' does not exist.");
                }
                throw new ArgumentException($"Column '{ColumnName}' appears {count} times, but nth={nth} is out of range.");
            }

        }
        public (int Rows, int Columns) Shape
        {
            get
            {
                return (index.Count, columns.Count);
            }
        }
        public int NumRows => frame[0].Count;
        public int NumColumns => columns.Count;
        // methods start here


    }
}

