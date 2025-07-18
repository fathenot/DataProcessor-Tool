﻿using DataProcessor.source.Index;
using DataProcessor.source.ValueStorage;
using System.Collections;
namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
      
        /// <summary>
        /// Initializes a new instance of the <see cref="Series"/> class, representing a one-dimensional array-like
        /// structure with associated index and data type.
        /// </summary>
        /// <remarks>The <see cref="Series"/> class is designed to handle structured data with an
        /// associated index, similar to a  one-dimensional array or a column in a table. The data type of the series
        /// can be explicitly specified or inferred  from the provided data. The index can be customized or
        /// automatically generated based on the data.  If <paramref name="data"/> is an <see cref="IDictionary"/>, the
        /// keys will be used as the index, and the values will  populate the series. If <paramref name="data"/> is an
        /// <see cref="IEnumerable"/>, its elements will populate the  series, and a default sequential index will be
        /// generated unless <paramref name="index"/> is provided.  The series supports various data types, including
        /// primitive types, <see cref="DateTime"/>, and <see cref="object"/>.  The index can also be customized to
        /// support grouped or multi-level indexing.</remarks>
        /// <param name="data">The data to populate the series. Must be either an <see cref="IDictionary"/> or an <see cref="IEnumerable"/>
        /// (excluding <see cref="string"/>). If <paramref name="data"/> is an <see cref="IDictionary"/>, the keys will
        /// be used  as the index, and the values will populate the series. If <paramref name="data"/> is an <see
        /// cref="IEnumerable"/>,  its elements will populate the series.</param>
        /// <param name="index">An optional collection of index values corresponding to the data. If provided, it must not contain null
        /// values.  If <paramref name="index"/> is not specified, a default sequential index will be generated.</param>
        /// <param name="dtype">An optional <see cref="Type"/> specifying the data type of the series. If not provided, the data type will
        /// be  inferred from the values in <paramref name="data"/>.</param>
        /// <param name="name">An optional name for the series. This can be used to identify the series in a larger context.</param>
        /// <param name="copy">A boolean value indicating whether to create a copy of the data. If <see langword="true"/>, the data will be
        /// copied; otherwise, the original data will be used directly.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is neither an <see cref="IDictionary"/> nor an <see cref="IEnumerable"/>,
        /// or if  <paramref name="index"/> contains null values.</exception>
        public Series(
          object? data = null,
          IEnumerable<object>? index = null,
          Type? dtype = null,
          string? name = null,
          bool copy = false)
        {
            List<object?> values;
            List<object>? finalIndex = null;

            // validate index must not contanin null if index is not null
            if (index != null)
            {
                // validate index must not contain null
                if (index.Any(v => v is null))
                {
                    throw new ArgumentException("index must not contain nulls", nameof(index));
                }
            }

            // handle data is dictionary or enumerable except string
            if (data is IDictionary dict)
            {
                values = new List<object?>();
                finalIndex = new List<object>();

                foreach (DictionaryEntry entry in dict)
                {
                    finalIndex.Add(entry.Key);
                    values.Add(entry.Value);
                }
            }

            else if (data is IEnumerable enumerable && !(data is string))
            {
                values = new List<object?>();
                foreach (var item in enumerable)
                {
                    if (item is null || item == DBNull.Value)
                    {
                        values.Add(null);
                    }
                    else
                    {
                        values.Add(item);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Data must be an IDictionary or IEnumerable.", nameof(data));
            }

            //select the type of data storage with the given type or infer from values
            this.dataType = dtype ?? Support.InferDataType(values);

            // create the value storage based on the values and data type
            this.values = ValueStorageCreate(values, copy);
            if(index == null) this.index = new RangeIndex(0, values.Count);
            else
            {
                // create the index based on the finalIndex
                this.index = CreateIndex(finalIndex);
            }
            this.seriesName = name;
        }
    }
}
