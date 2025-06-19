using DataProcessor.source.Index;

namespace DataProcessor.source.GenericsSeries
{
    // this partial class contains private methods for Series<DataType>
    public partial class Series<DataType> : ISeries<DataType> where DataType : notnull
    {
        /// <summary>
        /// Combines values and their corresponding indices into a collection of <see cref="IndexedValue"/> objects.
        /// </summary>
        /// <remarks>The method creates a new list with the same number of elements as the source
        /// collection of values. Each element in the resulting list represents a pairing of a value, cast to <see
        /// cref="DataType"/>, and its index.</remarks>
        /// <returns>A list of <see cref="IndexedValue"/> objects, where each object contains a value and its associated index.</returns>
        private List<IndexedValue> ZipIndexValue()
        {
            var result = new List<IndexedValue>(values.Count);
            for (int i = 0; i < values.Count; i++)
                result.Add((new IndexedValue((DataType)values[i], index[i])));
            return result;
        }
    }
}
