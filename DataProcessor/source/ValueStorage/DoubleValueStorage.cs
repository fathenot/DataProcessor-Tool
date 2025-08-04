using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// this class provides storage for nullable double values, allowing for efficient memory usage and null tracking.
    /// </summary>
    internal class DoubleValueStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly double[] array;
        NullBitMap nullBitMap;
        GCHandle handle;

        /// <summary>
        /// Validates that the specified index is within the bounds of the array.
        /// </summary>
        /// <param name="index">The index to validate. Must be greater than or equal to 0 and less than the length of the array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the array.</exception>
        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleValueStorage"/> class,  storing an array of nullable
        /// double values and tracking null entries.
        /// </summary>
        /// <remarks>This constructor processes the input array by converting non-null values to  <see
        /// cref="double"/> and storing them in an internal array. Null values are  represented using a null bitmap for
        /// efficient tracking. The input array is  pinned in memory using a <see cref="GCHandle"/> to ensure it remains
        /// fixed  during the lifetime of the instance.</remarks>
        /// <param name="array">An array of nullable <see cref="double"/> values to be stored.  Each null value in the input array is
        /// tracked using a null bitmap.</param>
        internal DoubleValueStorage(double?[] array)
        {
            this.array = new double[array.Length];
            nullBitMap = new NullBitMap(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    nullBitMap.SetNull(i, true);
                    this.array[i] = default;
                }
                else
                {
                    this.array[i] = Convert.ToDouble(array[i]);
                    nullBitMap.SetNull(i, false);
                }
                handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleValueStorage"/> class,  which provides storage for an
        /// array of double values with optional copying behavior.
        /// </summary>
        /// <remarks>If <paramref name="copy"/> is <see langword="false"/>, any modifications to the
        /// provided array  will directly affect the storage. If <paramref name="copy"/> is <see langword="true"/>,  the
        /// storage maintains its own independent copy of the array.</remarks>
        /// <param name="array">The array of double values to be stored. This array must not be null.</param>
        /// <param name="copy">A boolean value indicating whether the provided array should be copied.  If <see langword="true"/>, the
        /// array is copied to a new internal array.  If <see langword="false"/>, the provided array is used directly.
        /// The default is <see langword="true"/>.</param>
        internal DoubleValueStorage(double[] array, bool copy = true)
        {
            if (copy)
            {
                this.array = new double[array.Length];
                Array.Copy(array, this.array, array.Length);
            }
            else
            {
                this.array = array;
            }
            nullBitMap = new NullBitMap(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                nullBitMap.SetNull(i, false);
            }
            handle = GCHandle.Alloc(this.array, GCHandleType.Pinned);
        }
        internal override Type ElementType => typeof(double);

        internal override int Count => array.Length;
        internal override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }
        internal override object? GetValue(int index)
        {
            ValidateIndex(index);
            return nullBitMap.IsNull(index) ? null : array[index];
        }

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (nullBitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }

        internal override void SetValue(int index, object? value)
        {
            ValidateIndex(index);
            if (value is double || value is float)
            {
                array[index] = (double)value;
                nullBitMap.SetNull(index, false);
                return;
            }
            if (value is null)
            {
                array[index] = default;
                nullBitMap.SetNull(index, true);
                return;
            }
            throw new ArgumentException("Value must be of type double, float or null.", nameof(value));
        }

        ~DoubleValueStorage()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new DoubleEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        internal class DoubleEnumerator : IEnumerator<object?>
        {
            private readonly DoubleValueStorage storage;
            private int currentIndex = -1;
            internal DoubleEnumerator(DoubleValueStorage storage)
            {
                this.storage = storage;
            }
            public object? Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= storage.Count)
                    {
                        throw new InvalidOperationException("index is out of range");
                    }
                    return storage.GetValue(currentIndex);
                }
            }
            object IEnumerator.Current => Current!;
            public void Dispose() { }
            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < storage.Count;
            }
            public void Reset()
            {
                currentIndex = -1;
            }
        }

        /// <summary>
        /// Gets an array of non-null <see cref="double"/> values stored in this instance.
        /// </summary>
        /// <remarks>
        /// This property iterates through the internal storage, excluding any entries marked as null by the
        /// <c>_nullMap</c>. For each non-null element, it reconstructs the <see cref="double"/> from its corresponding
        /// tick value in the <c>_ticks</c> array. The resulting array contains only valid <see cref="double"/> values,
        /// and its length equals <c>Count - NullCount</c>.
        /// </remarks>
        /// <returns>
        /// An array of <see cref="double"/> values representing the non-null elements in the storage.
        /// </returns>
        internal double[] Values
        {
            get
            {
                double[] result = new double[this.Count - this.NullIndices.Count()];
                int resultIdx = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (!this.nullBitMap.IsNull(i))
                    {
                        result[resultIdx] = this.array[i];
                        resultIdx++;
                    }
                }
                return result;
            }
        }
    }
}
