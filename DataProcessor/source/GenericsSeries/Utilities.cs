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
            if (comparer == null)
            {
                comparer = Comparer<DataType>.Default;
                var sorted = indexedValues.OrderBy(x => x.Value, comparer).ToList();
                return new Series<DataType>(sorted.Select(x => x.Value).ToList(), this.name, sorted.Select(x => x.Index).ToList());
            }
            var sortedValues = indexedValues.OrderBy(x => x.Value, comparer).ToList();
            return new Series<DataType>(sortedValues.Select(x => x.Value).ToList(), this.name, sortedValues.Select(x => x.Index).ToList());
        }
        public SeriesView GetView(List<object> indicies)
        {
            return new SeriesView(this, indicies);
        }

        public SeriesView GetView((object start, object end, int step) slice)
        {
            return new SeriesView(this, slice);
        }

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

        public GroupView GroupByValue()
        {
            Dictionary<object, int[]> keyValuePairs = new Dictionary<object, int[]>();
            HashSet<DataType> removedDuplicate = new(values.Cast<DataType>().ToList());
            foreach (var ele in removedDuplicate)
            {
                keyValuePairs[ele] = this.values.Select((value, index) => new { value, index })
                            .Where(x => ele.Equals(x))
                            .Select(x => x.index)
                            .ToList().ToArray();
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

        public void CopyTo(DataType[] array, int arrayIndex)
        {
            values.Cast<DataType>().ToList().CopyTo(array, arrayIndex);
        }

        public Series ConvertToNonGenerics()
        {
            List<object?> values = new List<object?>();
            values.AddRange(this.values);
            return new Series(this.values, this.index, typeof(DataType), this.name);
        }
    }
}
