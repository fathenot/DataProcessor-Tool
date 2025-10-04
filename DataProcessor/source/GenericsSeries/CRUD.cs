using DataProcessor.source.Index;
using DataProcessor.source.NonGenericsSeries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
            this.values = new ValueStorage.GenericsStorage<DataType>(data.ToArray());
            this.name = name;

            if (index != null)
            {
                if (data.Count() != index.Count()) throw new ArgumentException($"Length of index must match length of data.", nameof(index));

                if (index.Count == 0 && data.Count == 0)
                {
                    this.index = new ObjectIndex(index);
                }
                else
                {
                    this.index = NonGenericsSeries.Series.CreateIndex(index);// reuse code generate index from non-generics series

                }
            }
            else this.index = new RangeIndex(0, Count - 1);
        }

        public Series(Series<DataType> other)
        {
            this.values = new ValueStorage.GenericsStorage<DataType>(other.values.ToArray());
            this.name = other.name;
            if (other.index.GetType() == typeof(RangeIndex))
            {
                this.index = (RangeIndex)other.index;
            }
            else this.index = NonGenericsSeries.Series.CreateIndex(other.index.IndexList.ToList());// reuse code generate index from non - generics series
        }
    }
}