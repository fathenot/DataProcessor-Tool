using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        // this partial class contains properties of the series

        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        public string? Name { get { return this.seriesName; } }

        /// <summary>
        /// Gets a read-only list of values stored in the collection.
        /// </summary>
        public IReadOnlyList<object?> Values
        {
            get
            {
                // return a read-only view of the values
                var readOnlyValues = new List<object?>(values.Count);
                for (int i = 0; i < values.Count; i++)
                {
                    readOnlyValues.Add(values.GetValue(i));
                }
                return readOnlyValues;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => values.Count;
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Gets the type of data associated with this instance.
        /// </summary>
        public Type DataType
        {
            get => dataType;
            private set => dataType = value;
        }

        /// <summary>
        /// Gets a list of values associated with the specified index.
        /// </summary>
        /// <remarks>This indexer retrieves all values mapped to the given index. If the index is not
        /// found, an exception is thrown.</remarks>
        /// <param name="index">The index to retrieve values for. Must exist in the collection.</param>
        /// <returns>A list of objects associated with the specified index. The list will contain all values mapped to the index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified <paramref name="index"/> does not exist in the collection.</exception>
        public List<object?> this[object index]
        {
            get
            {
                if (!this.index.Contains(index))
                {
                    throw new ArgumentOutOfRangeException("index not found", nameof(index));
                }
                List<object?> res = new List<object?>();
                foreach (int i in this.index.GetIndexPosition(index))
                {
                    res.Add(this.values.GetValue(i));
                }
                return res;
            }
        }

        /// <summary>
        /// Gets a list of objects representing the current index.
        /// </summary>
        public List<object> Index
        {
            get
            {
                return this.index.IndexList.ToList();
            }
        }
    }
}
