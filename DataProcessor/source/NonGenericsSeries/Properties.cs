namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        // this partial class contains properties of the series

        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        public string? Name => seriesName;

        /// <summary>
        /// Gets a read-only list of values stored in the collection.
        /// </summary>
        public IReadOnlyList<object?> Values
        {
            get
            {
                return valueStorage.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => valueStorage.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => true;

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
                    res.Add(this.valueStorage.GetValue(i));
                }
                return res;
            }
        }

        /// <summary>
        /// Gets a list of objects representing the current index.
        /// </summary>
        public List<object> Index => index.IndexList.ToList();
    }
}
