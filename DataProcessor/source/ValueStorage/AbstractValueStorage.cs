using System;
using System.Collections;
using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Provides an abstract representation of a columnar value storage unit,
    /// supporting typed access, null tracking, and native interop.
    /// </summary>
    internal abstract class AbstractValueStorage : IEnumerable<object?>
    {
        /// <summary>
        /// Gets the data type of the stored elements.
        /// </summary>
        internal abstract Type ElementType { get; }

        /// <summary>
        /// Gets the number of elements stored in the column.
        /// </summary>
        internal abstract int Count { get; }

        /// <summary>
        /// Gets a collection of indices where the corresponding element is null.
        /// </summary>
        internal abstract IEnumerable<int> NullIndices { get; }

        /// <summary>
        /// Retrieves the value at the specified logical position.
        /// </summary>
        internal abstract object? GetValue(int index);

        /// <summary>
        /// Sets the value at the specified logical index.
        /// </summary>
        internal abstract void SetValue(int index, object? value);

        /// <summary>
        /// Gets the memory address of the underlying native array.
        /// </summary>
        internal abstract nint GetNativeBufferPointer();

        /// <summary>
        /// Gets an enumerator for iterating over the storage.
        /// </summary>
        public abstract IEnumerator<object?> GetEnumerator();

        /// <summary>
        /// Required for IEnumerable compatibility with non-generic consumers.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
