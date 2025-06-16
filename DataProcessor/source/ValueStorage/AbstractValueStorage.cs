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

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <returns></returns>
        internal object? this[int index]
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }

        /// <summary>
        /// Enumerates the elements of the collection that can be cast to the specified type.
        /// </summary>
        /// <remarks>Elements that cannot be cast to the specified type are skipped. This method uses
        /// deferred execution.</remarks>
        /// <typeparam name="T">The type to which the elements should be cast.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the elements of the collection that are successfully cast to type
        /// <typeparamref name="T"/>.</returns>
        public IEnumerable<T?> AsTyped<T>()
        {
            for (int i = 0; i < Count; i++)
            {
                var value = GetValue(i);

                if (value is null)
                {
                    yield return default; // null vẫn là T? nếu T là struct
                }
                else if (value is T t)
                {
                    yield return t;
                }
                else
                {
                    throw new InvalidCastException($"Cannot cast value at index {i} to type {typeof(T)}.");
                }
            }
        }

    }
}
