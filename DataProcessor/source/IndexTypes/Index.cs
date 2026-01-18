using System;
using System.Collections;
using System.Collections.Generic;

namespace DataProcessor.source.IndexTypes
{
    /// <summary>
    /// Represents an abstract base class for indexing functionality in a DataFrame-like structure.
    /// Supports lookup, slicing, and metadata about the index.
    /// </summary>
    public abstract class DataIndex : IEnumerable<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// This constructor is reserved for internal use by derived classes.
        /// </summary>
        protected DataIndex() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class using a list of index values.
        /// This constructor is optional and may be used by subclasses for convenience.
        /// </summary>
        /// <param name="indexList">The list of index values.</param>
        protected DataIndex(List<object> indexList) { }

        /// <summary>
        /// Gets the number of elements in the index.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets the full list of index values as an immutable list.
        /// </summary>
        public abstract IReadOnlyList<object> IndexList { get; }

        /// <summary>
        /// Retrieves the index value at the specified position.
        /// </summary>
        /// <param name="idx">The zero-based position in the index.</param>
        /// <returns>The index value at the specified position.</returns>
        public abstract object GetIndex(int idx);

        /// <summary>
        /// Gets all positions in the index that match the specified value.
        /// </summary>
        /// <param name="index">The value to locate in the index.</param>
        /// <returns>A list of positions where the value occurs.</returns>
        public abstract IReadOnlyList<int> GetIndexPosition(object index);

        /// <summary>
        /// Determines whether the index contains the specified key.
        /// </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public abstract bool Contains(object key);

        /// <summary>
        /// Gets the first position of the specified key in the index.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The zero-based position of the first occurrence, or -1 if not found.</returns>
        public abstract int FirstPositionOf(object key);

        /// <summary>
        /// Creates a new index that represents a slice of the current index.
        /// </summary>
        /// <param name="start">The starting position (inclusive).</param>
        /// <param name="end">The ending position (exclusive).</param>
        /// <param name="step">The step between elements (default is 1).</param>
        /// <returns>A new <see cref="DataIndex"/> containing the sliced values.</returns>
        public abstract DataIndex Slice(int start, int end, int step = 1);

        /// <summary>
        /// Creates a new index by extracting elements from the current index based on the specified list of keys.
        /// </summary>
        /// <param name="indexList">A list of keys used to select elements from the current index. Each key must correspond to an existing
        /// element in the index.</param>
        /// <returns>An <see cref="Index"/> containing the elements that match the specified keys.</returns>

        public abstract DataIndex TakeKeys(List<object> indexList);
        /// <summary>
        /// Gets all distinct index values in the current index.
        /// </summary>
        /// <returns>An enumerable of distinct index values.</returns>
        public abstract IEnumerable<object> DistinctIndices();

        /// <summary>
        /// Returns an enumerator that iterates through the index values.
        /// </summary>
        /// <returns>An enumerator for the index.</returns>
        public abstract IEnumerator<object> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the value at the specified index in the collection.
        /// </summary>
        /// <remarks>This property provides read-only access to the collection. To modify values, use a
        /// derived class that supports setting index values.</remarks>
        /// <param name="index">The zero-based index of the value to retrieve.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual object this[int index]
        {
            get => GetIndex(index);
            protected set
            {
                throw new NotSupportedException("Setting index values is not supported in Index. Use derived classes for specific implementations.");
            }
        }

        public abstract DataIndex Clone();
    }
}
