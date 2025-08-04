using DataProcessor.source.ValueStorage;
namespace DataProcessor.source.EngineWrapper.SortingEngine
{
    internal class IndexValueSorter
    {
        public static void SortByIndex<T>(T[] array, List<object> index, bool ascending = true)
        {
            if (array.Length != index.Count)
                throw new ArgumentException("Index and array must have the same length.");

            if (array.Length <= 1) return;
            // Create a mapping of index to array elements
            var indexedArray = array.Select((value, idx) => new { Value = value, Index = index[idx] }).ToList();
            // Sort the indexed array based on the index values
            indexedArray.Sort((x, y) => Comparer<object>.Default.Compare(x.Index, y.Index));
            // If descending order is required, reverse the sorted list
            if (!ascending)
                indexedArray.Reverse();
            // Update the original array with sorted values
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = indexedArray[i].Value;
            }
        }

        public static void SortByValue<T>(T[] array, List<object> index, bool ascending = true)
        {
            if (array.Length != index.Count)
                throw new ArgumentException("Index and array must have the same length.");

            if (array.Length <= 1) return;
            // Create a mapping of index to array elements
            var indexedArray = array.Select((value, idx) => new { Value = value, Index = index[idx] }).ToList();
            // Sort the indexed array based on the values
            indexedArray.Sort((x, y) => Comparer<object>.Default.Compare(x.Value, y.Value));
            // If descending order is required, reverse the sorted list
            if (!ascending)
                indexedArray.Reverse();
            // Update the original array with sorted values
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = indexedArray[i].Value;
            }
        }

    }
}
