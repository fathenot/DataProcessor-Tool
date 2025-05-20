using System;
using System.Collections.Generic;

namespace DataProcessor.source.Index
{
    /// <summary>
    /// Represents an abstract base class for indexing functionality in a DataFrame-like structure.
    /// Supports lookup, slicing, and metadata about the index.
    /// </summary>
    public abstract class IIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IIndex"/> class.
        /// This constructor is reserved for internal use by derived classes.
        /// </summary>
        protected IIndex() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IIndex"/> class using a list of index values.
        /// This constructor is optional and may be used by subclasses for convenience.
        /// </summary>
        /// <param name="indexList">The list of index values.</param>
        protected IIndex(List<object> indexList) { }

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
        /// <returns>A new <see cref="IIndex"/> containing the sliced values.</returns>
        public abstract IIndex Slice(int start, int end, int step = 1);

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
    }
}
