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
    }
}
