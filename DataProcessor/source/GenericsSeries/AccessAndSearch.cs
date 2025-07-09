using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.GenericsSeries
{
    public partial class Series<DataType>
    {
        /// <summary>
        /// Retrieves the first <paramref name="count"/> elements from the series.
        /// </summary>
        /// <param name="count">The number of elements to retrieve. Must be greater than or equal to 0.</param>
        /// <returns>A new <see cref="Series{DataType}"/> containing the first <paramref name="count"/> elements of the series. If <paramref
        /// name="count"/> exceeds the number of elements in the series, the entire series is returned.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 0.</exception>
        public Series<DataType> Head(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");
            return new Series<DataType>(values.Take(count).Cast<DataType>().ToList(), index: index.IndexList.Take(count).ToList());
        }

        /// <summary>
        /// Retrieves the last <paramref name="count"/> elements from the series.
        /// </summary>
        /// <param name="count">The number of elements to retrieve. Must be greater than or equal to 0.</param>
        /// <returns>A new <see cref="Series{DataType}"/> containing the last <paramref name="count"/> elements. If <paramref
        /// name="count"/> exceeds the number of elements in the series, the entire series is returned.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 0.</exception>
        public Series<DataType> Tail(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");
            return new Series<DataType>(values.Skip(Math.Max(0, values.Count - count)).Cast<DataType>().ToList(), 
                index: index.IndexList.Skip(Math.Max(0, index.IndexList.Count - count)).ToList());
        }

        /// <summary>
        /// Filters the elements of the series based on a specified predicate.
        /// </summary>
        /// <remarks>The filtered series retains the original indices of the elements that satisfy the
        /// predicate.</remarks>
        /// <param name="filter">A function that defines the condition to filter the elements. The function takes an element of type
        /// <typeparamref name="DataType"/>  and returns <see langword="true"/> to include the element in the filtered
        /// series; otherwise, <see langword="false"/>.</param>
        /// <returns>A new <see cref="Series{DataType}"/> containing only the elements that satisfy the specified predicate.</returns>
        public Series<DataType> Filter(Func<DataType, bool> filter)
        {
            var zipped = values.Zip(index.IndexList, (value, idx) => new { Value = value, Index = idx });
            zipped = zipped.Where(x => filter((DataType)x.Value)).ToList();
            return new Series<DataType>(zipped.Select(x => (DataType)x.Value).ToList(), name, zipped.Select(x => x.Index).ToList());
        }

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <remarks> This method performs a search to determine whether the specified item exists in the
        /// collection. The search behavior depends on the implementation of the underlying collection. </remarks>
        /// <param name="item">The item to locate in the collection.</param>
        /// <returns><see langword="true"/> if the specified item is found in the collection; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(DataType item)
        {
            return values.Contains(item);
        }

        /// <summary>
        /// Finds the first occurrence of the specified item in the collection.
        /// </summary>
        /// <remarks>The method iterates through the collection to locate the first occurrence of the
        /// specified item. If a custom <paramref name="comparer"/> is provided, it is used to determine equality;
        /// otherwise, the default equality comparer is used.</remarks>
        /// <param name="item">The item to search for in the collection.</param>
        /// <param name="comparer">An optional equality comparer to use for comparing items. If <see langword="null"/>, the default equality
        /// comparer for <typeparamref name="DataType"/> is used.</param>
        /// <returns>A tuple containing the value of the first matching item and its associated index, or <see langword="null"/>
        /// if no matching item is found.</returns>
        public (DataType value, object index)? FindFirstOccur(DataType item, IEqualityComparer<DataType>? comparer = null)
        {
            for(int i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<DataType>.Default.Equals((DataType)values[i], item))
                {
                    return ((DataType)values[i], index.IndexList[i]);
                }
            }
            return null;
        }
    }
}
