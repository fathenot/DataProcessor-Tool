using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.NonGenericsSeries;
namespace DataProcessor.source.GenericsSeries
{
   

    public partial class Series<DataType>
    {
        // utility methods
        
        /// <summary>
        /// Sorts the series based on the values, using the specified comparer or the default comparer.
        /// </summary>
        /// <remarks>The method returns a new series where the values are sorted in ascending order based
        /// on the provided comparer. If no comparer is specified, the default comparer for the type <typeparamref
        /// name="DataType"/> is used. The indices in the returned series correspond to the original positions of the
        /// values in the unsorted series.</remarks>
        /// <param name="comparer">An optional <see cref="Comparer{T}"/> used to compare the values in the series. If <paramref
        /// name="comparer"/> is null, the default comparer for the type <typeparamref name="DataType"/> is used.</param>
        /// <returns>A new <see cref="Series{DataType}"/> instance containing the values sorted in ascending order, along with
        /// their corresponding indices.</returns>
        public Series<DataType> Sort(Comparer<DataType>? comparer = null)
        {
            // generate list of indexed values
            List<IndexedValue> indexedValues = this.ZipIndexValue();
            // sort indexed values based on values
            indexedValues.Sort((x, y) => (comparer ?? Comparer<DataType>.Default).Compare(x.Value, y.Value));
            return new Series<DataType>(indexedValues.Select(x => x.Value).ToList(), this.name, indexedValues.Select(x => x.Index).ToList());
        }

        /// <summary>
        /// Create a view from series
        /// </summary>
        /// <param name="indicies"></param>
        /// <returns></returns>
        public SeriesView GetView(List<object> indicies)
        {
            return new SeriesView(this, indicies);
        }

        /// <summary>
        /// Create a view from series
        /// </summary>
        /// <param name="indicies"></param>
        /// <returns></returns>
        public SeriesView GetView((object start, object end, int step) slice)
        {
            return new SeriesView(this, slice);
        }

       /// <summary>
       /// Groups the elements of the current object by their index values and returns a view of the grouped data.
       /// </summary>
       /// <remarks>This method identifies distinct index values from the current object's index list,
       /// determines the positions of each index, and groups the elements accordingly. The resulting groups are
       /// returned as a <see cref="GroupView"/>.</remarks>
       /// <returns>A <see cref="GroupView"/> representing the grouped elements, where each group is keyed by its index value and
       /// contains the positions of the elements in the group.</returns>
        public GroupView GroupsByIndex()
        {
            Dictionary<object, int[]> keyValuePairs = new Dictionary<object, int[]>();
            foreach (var index in this.index.IndexList.Distinct())
            {
                // get the index position
                var positions = this.index.GetIndexPosition(index);
                if (positions.Count > 0)
                {
                    keyValuePairs[index] = positions.ToArray();
                }
            }
            return new GroupView(this, keyValuePairs);
        }

        /// <summary>
        /// Counts the number of elements in each group.
        /// </summary>
        /// <returns>
        /// A dictionary mapping each group key to the number of elements it contains.
        /// </returns>
        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> keyValuePairs = new Dictionary<object, int[]>();
            HashSet<DataType> removedDuplicate = new(values.Cast<DataType>().ToList());
            foreach (var ele in removedDuplicate)
            {
                keyValuePairs[ele] = this.values.Select((value, index) => new { value, index })
                            .Where(x => ele.Equals(x.value))
                            .Select(x => x.index)
                            .ToArray();
            }
            return new GroupView(this, keyValuePairs);
        }

        // copy
        public Series<DataType> Clone()
        {
            return new Series<DataType>(
                new List<DataType>(this.values.Cast<DataType>()),
                this.name,
                new List<object>(this.index.IndexList)
            );
        }

        /// <summary>
        /// Copies the elements of the collection to a specified array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must
        /// have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.ToList().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Converts the current generic series to a non-generic <see cref="Series"/> instance.
        /// </summary>
        /// <remarks>The returned <see cref="Series"/> will have the same values, index, and name as the
        /// current instance, but will not be strongly typed to a specific data type.</remarks>
        /// <returns>A non-generic <see cref="Series"/> instance containing the same data as the current series.</returns>
        public Series ConvertToNonGenerics()
        {
            return new Series(this.values, this.index, typeof(DataType), this.name, copy:true);
        }

        /// <summary>
        /// Applies the specified aggregation function to each element in the series and returns a new series containing
        /// the aggregated results.
        /// </summary>
        /// <remarks>The returned series will have the same index as the original series, but the values
        /// will be transformed based on the provided aggregation function.</remarks>
        /// <typeparam name="Tresult">The type of the result produced by the aggregation function.</typeparam>
        /// <param name="function">A function that takes an element of type <typeparamref name="DataType"/> and returns a value of type
        /// <typeparamref name="Tresult"/>.</param>
        /// <returns>A new <see cref="Series{Tresult}"/> containing the results of applying the specified function to each
        /// element in the series.</returns>
        public Series<Tresult> Aggregate<Tresult>(Func<DataType, Tresult> function)
        {
            List<Tresult> aggregatedValues = this.values.Select(function).ToList();
            return new Series<Tresult>(aggregatedValues, null, this.index.IndexList.ToList());
        }
    }
}
