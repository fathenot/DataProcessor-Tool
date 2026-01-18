using DataProcessor.source.UserSettings.DefaultValsGenerator;
using DataProcessor.source.ValueStorage;
using System.Collections;
using DataProcessor.source.IndexTypes;
using System.Text;

namespace DataProcessor.source.GenericsSeries
{
    /// <summary>
    /// Represents a collection of data values indexed by a customizable index type.
    /// </summary>
    /// <remarks>The <see cref="Series{DataType}"/> class provides functionality for managing and manipulating
    /// a collection of data values indexed by a customizable index type. It supports operations such as filtering,
    /// slicing, grouping, and aggregation. The series is thread-safe for read and write operations using a <see
    /// cref="ReaderWriterLock"/> mechanism.  This class is designed to handle scenarios where data is associated with
    /// non-numeric or complex indices, such as time-series data or categorical data. It allows for flexible indexing
    /// and provides views for subsets of data, as well as group-based operations.</remarks>
    /// <typeparam name="DataType">The type of data stored in the series. Must be a non-nullable type.</typeparam>
    public partial class Series<DataType> : ISeries<DataType>
    {
        private string? name;
        private GenericsStorage<DataType> values; // this is the storage of the series values
        private DataIndex index; // this is the index of the series, it can be any type of index

        public sealed class GenericsSeriesEnumerator : IEnumerator<DataType>
        {
            private readonly Series<DataType> series;
            private int _currentIndex = -1;

            public GenericsSeriesEnumerator(Series<DataType> series)
            {
                this.series = series;
            }

            public DataType Current => series.values.GetValue(_currentIndex);

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < series.Count;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose()
            { 
                //no op
            }
        }

        internal record struct IndexedValue(DataType Value, object Index);
    }
}
