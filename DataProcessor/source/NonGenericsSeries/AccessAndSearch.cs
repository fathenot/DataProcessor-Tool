using DataProcessor.source.GenericsSeries;
using System.ComponentModel.DataAnnotations;

namespace DataProcessor.source.NonGenericsSeries
{
    // this place contains methods for accessing and searching the series
    public partial class Series
    {

        /// <summary>
        /// Retrieves the first <paramref name="count"/> elements from the series.
        /// </summary>
        /// <param name="count">The number of elements to retrieve. Must be between 0 and the total number of elements in the series.</param>
        /// <returns>A new <see cref="ISeries"/> containing the first <paramref name="count"/> elements of the series.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 0 or greater than the total number of elements in the
        /// series.</exception>
        public Series Head(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");
            return new Series (values.Take(count).ToList(),
                index: index.IndexList.Take(count).ToList());
        }

        /// <summary>
        /// Returns a new series containing the last <paramref name="count"/> elements of the current series.
        /// </summary>
        /// <remarks>This method creates a new series by extracting the specified number of elements from
        /// the end of the current series. The resulting series retains the original index and series name.</remarks>
        /// <param name="count">The number of elements to include in the resulting series. Must be between 0 and the total number of
        /// elements in the current series.</param>
        /// <returns>A new <see cref="ISeries"/> instance containing the last <paramref name="count"/> elements of the current
        /// series.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 0 or greater than the total number of elements in the
        /// current series.</exception>
        public Series Tail(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");
            return new Series (values.Skip(Math.Max(0, values.Count - count)).ToList(),
                index: index.IndexList.Skip(Math.Max(0, index.IndexList.Count - count)).ToList());
        }

        /// <summary>
        /// Retrieves a view of the series based on the specified range and step size.
        /// </summary>
        /// <remarks>The method generates a view of the series by iterating over the specified range of
        /// indices. If <paramref name="slices.step"/> is positive, the iteration proceeds from <paramref
        /// name="slices.start"/> to <paramref name="slices.end"/> inclusively. If <paramref name="slices.step"/> is
        /// negative, the iteration proceeds in reverse order.</remarks>
        /// <param name="slices">A tuple containing the start index, end index, and step size for the view. <paramref name="slices.start"/>
        /// and <paramref name="slices.end"/> must exist in the series index. <paramref name="slices.step"/> specifies
        /// the interval between indices and cannot be zero.</param>
        /// <returns>A <see cref="SeriesView"/> object representing the subset of the series defined by the specified range and
        /// step.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="slices.start"/> or <paramref name="slices.end"/> does not exist in the series
        /// index, or if <paramref name="slices.step"/> is zero.</exception>
        public SeriesView GetView((object start, object end, int step) slices)
        {
            // check valid argument
            if (!this.index.Contains(slices.start) || !this.index.Contains(slices.end))
            {
                throw new ArgumentException("Start or end index does not exist in the series index.");
            }
            if (slices.step == 0)
            {
                throw new ArgumentException("Step cannot be zero.");
            }

            return new SeriesView(this, slices);
        }

        /// <summary>
        /// Creates a view of the current series based on the specified slice.
        /// </summary>
        /// <remarks>The <paramref name="slice"/> parameter determines how the series is segmented or
        /// filtered in the resulting view. Ensure that the objects in the slice are compatible with the series
        /// structure.</remarks>
        /// <param name="slice">A list of objects representing the slice to be used for creating the view. Each object in the list defines a
        /// segment or filter criteria for the view.</param>
        /// <returns>A <see cref="SeriesView"/> instance that represents the view of the series based on the provided slice.</returns>
        public SeriesView GetView(List<object> slice)
        {
            return new SeriesView(this, slice);
        }

        // searching and filter

        /// <summary>
        /// Filters the elements of the collection based on a specified predicate.
        /// </summary>
        /// <remarks>This method iterates through all elements in the collection and applies the <paramref
        /// name="filter"/> function to each. The performance of this method depends on the size of the collection and
        /// the complexity of the predicate.</remarks>
        /// <param name="filter">A function that defines the condition to determine whether an element should be included in the result. The
        /// function takes an element of the collection as input and returns <see langword="true"/> to include the
        /// element; otherwise, <see langword="false"/>.</param>
        /// <returns>A read-only list containing the elements that satisfy the specified predicate. If no elements match the
        /// predicate, the returned list will be empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filter"/> is <see langword="null"/>.</exception>
        public IList<object?> Filter(Func<object?, bool> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter function cannot be null.");
            }

            List<object?> filteredValues = new List<object?>();
            for (int i = 0; i < this.Count; i++)
            {
                if (filter(this.values.GetValue(i)))
                {
                    filteredValues.Add(this.values.GetValue(i));
                }
            }

            return filteredValues.AsReadOnly();
        }


        /// <summary>
        /// Searches for all occurrences of the specified item in the collection and returns their indices.
        /// </summary>
        /// <remarks>The method performs a linear search and compares items using <see
        /// cref="object.Equals(object, object)"/>.</remarks>
        /// <param name="item">The object to locate in the collection. Can be <see langword="null"/>.</param>
        /// <returns>A list of integers representing the indices of all occurrences of <paramref name="item"/> in the collection.
        /// Returns an empty list if the item is not found.</returns>
        public List<int> Find(object? item)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (object.Equals(this.values.GetValue(i), item))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <remarks>This method checks for the presence of the specified item in the collection. The
        /// comparison may depend on the implementation of the collection and the equality logic for the item.</remarks>
        /// <param name="item">The object to locate in the collection. The value can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the specified item is found in the collection; otherwise, <see langword="false"/>.</returns>

        public bool Contains(object? item)
        {
            return this.values.Contains(item);
        }
    }
}
