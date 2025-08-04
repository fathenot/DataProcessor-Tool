using System.Collections;

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Represents a generic, type-safe value storage implementation for the DataSharp engine.
    /// Stores values of type <typeparamref name="T"/> with null tracking using a bitmap.
    /// </summary>
    /// <typeparam name="T">The data type to store in this storage.</typeparam>
    internal class GenericsStorage<T> : IEnumerable<T>
    {
        /// <summary>
        /// The array of values stored in this storage instance.
        /// </summary>
        private readonly T[] values;

        /// <summary>
        /// Bitmap used to track null values at each position.
        /// </summary>
        private readonly NullBitMap nullBitMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsStorage{T}"/> class from a list of values.
        /// Automatically detects and marks nulls in the null bitmap.
        /// </summary>
        /// <param name="values">The list of values to store.</param>
        internal GenericsStorage(List<T> values)
        {
            this.values = values.ToArray();
            nullBitMap = new NullBitMap(values.Count);

            for (int i = 0; i < values.Count; i++)
            {
                nullBitMap.SetNull(i, values[i] == null);
            }
        }

        internal GenericsStorage(T[] values)
        {
            this.values = new T[values.Length];
            nullBitMap = new NullBitMap(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                this.values[i] = values[i];
                nullBitMap.SetNull(i, values[i] == null);
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        internal int Count => values.Length;

        internal T this[int index]
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }
        /// <summary>
        /// Gets the type of the elements contained in the collection.
        /// </summary>
        internal Type ElementType => typeof(T);

        /// <summary>
        /// Retrieves the value at the specified index in the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the value to retrieve. Must be within the bounds of the collection.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the collection.</exception>
        internal T GetValue(int index)
        {
            if (index < 0 || index >= values.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return values[index];
        }

        /// <summary>
        /// Sets the value at the specified index in the collection.
        /// </summary>
        /// <remarks>If <paramref name="value"/> is <see langword="null"/>, the corresponding index is
        /// marked as null. Otherwise, the value is stored and the null marker is cleared.</remarks>
        /// <param name="index">The zero-based index at which to set the value. Must be within the bounds of the collection.</param>
        /// <param name="value">The value to set at the specified index. Must be of type <typeparamref name="T"/> or <see langword="null"/>.
        /// If <see langword="null"/>, the value at the index is cleared.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the size of the collection.</exception>
        /// <exception cref="InvalidCastException">Thrown if <paramref name="value"/> is not of type <typeparamref name="T"/> and is not <see
        /// langword="null"/>.</exception>
        internal void SetValue(int index, object? value)
        {
            if (index < 0 || index >= values.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (value is T typedValue)
            {
                values[index] = typedValue;
                nullBitMap.SetNull(index, false);
            }
            else if (value is null)
            {
                values[index] = default!;
                nullBitMap.SetNull(index, true);
            }
            else
            {
                throw new ArgumentException($"Expected a value of type {typeof(T)}");
            }
        }

        /// <summary>
        /// Gets the indexes of the null values in this storage.
        /// </summary>
        internal IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (nullBitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates over a collection of generic values, providing support for nullability.
        /// </summary>
        /// <remarks>This enumerator iterates through an array of values, using a null bitmap to determine
        /// whether each value is null. The <see cref="Current"/> property returns the current value in the enumeration,
        /// or <see langword="null"/> if the corresponding entry in the null bitmap indicates a null value.</remarks>
        private sealed class GenericsValueEnumerator : IEnumerator<T>
        {
            private readonly T[] values;
            private readonly NullBitMap nullBitMap;
            private int currentIndex = -1;

            public GenericsValueEnumerator(T[] values, NullBitMap nullBitMap)
            {
                this.values = values;
                this.nullBitMap = nullBitMap;
            }

            public T Current => nullBitMap.IsNull(currentIndex) ? default(T) : values[currentIndex];

            object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < values.Length;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public void Dispose() { }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>The enumerator provides access to each element in the collection in sequence.  Use
        /// this method to perform iteration using a `foreach` loop or manual enumeration.</remarks>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new GenericsValueEnumerator(values, nullBitMap);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// gets an array of values stored in this storage
        /// </summary>
        internal T[] Values
        {
            get
            {
                T[] copy = new T[values.Length];
                Array.Copy(values, copy, values.Length);
                return copy;
            }
        }
    }
}
