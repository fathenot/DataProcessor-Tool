namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Provides an abstract representation of a columnar value storage unit,
    /// supporting typed access, null tracking, and native interop.
    /// </summary>
    internal abstract class ValueStorage
    {
        /// <summary>
        /// Gets the data type of the stored elements.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Gets the number of elements stored in the column.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets a collection of indices where the corresponding element is null.
        /// </summary>
        public abstract IEnumerable<int> NullIndices { get; }

        /// <summary>
        /// Retrieves the value at the specified logical position.
        /// </summary>
        /// <param name="index">The logical index of the element.</param>
        /// <returns>The boxed value at the specified index, or null if unset.</returns>
        public abstract object? GetValue(int index);

        /// <summary>
        /// Sets the value at the specified logical index.
        /// </summary>
        /// <param name="index">The index of the element to set.</param>
        /// <param name="value">The new value to assign, or null to mark as missing.</param>
        public abstract void SetValue(int index, object? value);

        /// <summary>
        /// Gets the memory address of the underlying native array,
        /// primarily for C++ interop or unsafe operations.
        /// </summary>
        /// <returns>A raw pointer to the underlying pinned buffer.</returns>
        public abstract nint GetNativeBufferPointer();

    }
}
