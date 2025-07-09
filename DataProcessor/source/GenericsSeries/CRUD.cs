using DataProcessor.source.Index;
using DataProcessor.source.NonGenericsSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.GenericsSeries
{ 
    // This partial class contains crud operations for series. Currently it only contains constructors
    public partial class Series<DataType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Series{DataType}"/> class, representing a collection of data
        /// points with an optional name and index.
        /// </summary>
        /// <remarks>The <paramref name="index"/> parameter is validated to ensure it does not contain
        /// null values. Based on the type of the index elements, an appropriate index implementation is selected, such
        /// as <see cref="Index.StringIndex"/>, <see cref="Index.Int64Index"/>, or <see cref="Index.MultiIndex"/> If the
        /// index contains grouped elements (e.g., arrays), a <see cref="Index.MultiIndex"/> is created.</remarks>
        /// <param name="data">The collection of data points to be stored in the series. Cannot be null.</param>
        /// <param name="name">The optional name of the series. If not provided, defaults to an empty string.</param>
        /// <param name="index">The optional index associated with the data points. If not provided, a default range index is created. The
        /// index must not contain null values, and its type determines the specific index implementation.</param>
        public Series(List<DataType> data, string? name = null, List<object>? index = null)
        {
            this.name = name ?? string.Empty;
            this.values = new ValueStorage.GenericsStorage<DataType>(data);
            if(index == null)
            {
                this.index = new RangeIndex(0, data.Count - 1);
            }
            else
            {
                // validate index must not contain null
                var finalIndex = index.Cast<object>().ToList();

                // check type of index and create index
                bool containGroups = DataProcessor.source.Index.IndexUtils.ContainsGroupedIndex(index);
                if (containGroups)
                {
                    this.index = new Index.MultiIndex(index.Select(i => i is object[]? new Index.MultiKey((object[])i) : new Index.MultiKey(new object[] { i })).ToList());
                }

                else if (Support.InferDataType(finalIndex) == typeof(string))
                {
                    this.index = new Index.StringIndex(index.Cast<string>().ToList());
                }
                else if (Support.InferDataType(finalIndex) == typeof(long))
                {
                    this.index = new Index.Int64Index(index.Cast<long>().ToList());
                }
                else if (Support.InferDataType(finalIndex) == typeof(double))
                {
                    this.index = new Index.DoubleIndex(index.Cast<double>().ToList());
                }
                else if (Support.InferDataType(finalIndex) == typeof(decimal))
                {
                    this.index = new Index.DecimalIndex(index.Cast<decimal>().ToList());
                }

                else if (Support.InferDataType(finalIndex) == typeof(DateTime))
                {
                    this.index = new Index.DateTimeIndex(finalIndex.Cast<DateTime>().ToList());
                }

                else if (Support.InferDataType(finalIndex) == typeof(char))
                {
                    this.index = new Index.CharIndex(index.Cast<char>().ToList());
                }

                else if (Support.InferDataType(finalIndex) == typeof(object))
                {
                    this.index = new Index.ObjectIndex(finalIndex);
                }
            }
        }
    }
}