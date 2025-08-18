using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class Int32ValuesStorage: AbstractValueStorage, IEnumerable<object?>
    {
        private readonly int[] _intValues;
        private readonly NullBitMap _nullBitMap;
        private readonly GCHandle _handle;
        private bool disposed = false;

        /// <summary>
        /// Validates whether the specified index is within the bounds of the internal collection.
        /// </summary>
        /// <param name="index">The index to validate. Must be greater than or equal to 0 and less than the length of the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the collection.</exception>
        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _intValues.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int64ValuesStorage"/> class, storing a collection of nullable
        /// integers and tracking their nullability using a bitmap.
        /// </summary>
        /// <remarks>This constructor initializes the internal storage for integer values and a bitmap to
        /// track which elements are null. The provided array is processed such that non-null values are stored in an
        /// internal array, and null values are recorded in the bitmap. The internal array is pinned in memory to ensure
        /// it remains accessible for unmanaged operations.</remarks>
        /// <param name="values">An array of nullable integers to be stored. Each element represents a value or a null entry.</param>
        internal Int32ValuesStorage(int?[] values)
        {
            _intValues = new int[values.Length];
            _nullBitMap = new NullBitMap(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    _intValues[i] = values[i].Value;
                }
                _nullBitMap.SetNull(i, !values[i].HasValue);
            }

            _handle = GCHandle.Alloc(_intValues, GCHandleType.Pinned);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int64ValuesStorage"/> class with the specified values.
        /// </summary>
        /// <remarks>If <paramref name="copy"/> is <see langword="false"/>, the caller must ensure that
        /// the <paramref name="values"/> array is not modified externally after being passed to this constructor, as
        /// the instance will directly reference it. The constructor also initializes a bitmap to track nullability,
        /// marking all values as non-null.</remarks>
        /// <param name="values">An array of <see langword="int"/> values to be stored. This array must not be <see langword="null"/>.</param>
        /// <param name="copy">A <see langword="bool"/> indicating whether to create a copy of the <paramref name="values"/> array. If <see
        /// langword="true"/>, the values are copied into a new array; otherwise, the provided array is used directly.</param>
        internal Int32ValuesStorage(int[] values, bool copy = false)
        {
            if (copy)
            {
                _intValues = new int[values.Length];
                Array.Copy(values, _intValues, values.Length);
            }
            else
            {
                _intValues = values;
            }
            _nullBitMap = new NullBitMap(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                _nullBitMap.SetNull(i, false);
            }
            _handle = GCHandle.Alloc(_intValues, GCHandleType.Pinned);
        }

        /// <summary>
        /// Gets an array containing all non-null int values in the collection.
        /// </summary>
        internal int[] NonNullValues
        {
            get
            {
                int[] result = new int[_intValues.Length - NullIndices.Count()];
                int current_idx = 0;
                for (int i = 0; i < _intValues.Length; i++)
                {
                    if (!_nullBitMap.IsNull(i))
                    {
                        result[current_idx] = _intValues[i];
                        current_idx++;
                    }
                }
                return result;
            }
        }
        internal override int Count => _intValues.Length;

        internal override StorageKind storageKind => StorageKind.Int32;
        internal override Type ElementType => typeof(int);

        internal int CountNullValues => _nullBitMap.CountNulls();

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < _intValues.Length; i++)
                {
                    if (_nullBitMap.IsNull(i))
                        yield return i;
                }
            }
        }

        internal override object? GetValue(int index)
        {
            ValidateIndex(index);
            return _nullBitMap.IsNull(index) ? null : _intValues[index];
        }
        internal override void SetValue(int index, object? value)
        {

            ValidateIndex(index);
            if (value is null)
            {
                _nullBitMap.SetNull(index, true);
                _intValues[index] = default;
                return;
            }
            if (value is IConvertible convertible)
            {
                try
                {
                    _intValues[index] = Convert.ToInt32(convertible);
                    _nullBitMap.SetNull(index, false);
                    return;
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Cannot convert value to int: {e.Message}", e);
                }
            }

            throw new ArgumentException("Value must be a numeric type or null.");
        }

        internal override nint GetNativeBufferPointer() => _handle.AddrOfPinnedObject();

        public override IEnumerator<object?> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // this part used to use IDisposable, but now we use the Dispose pattern

        public void Dispose()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }

                disposed = true;
            }
        }
        ~Int32ValuesStorage()
        {
            Dispose();
        }

        /// <summary>
        /// Enumerates the elements of an <see cref="Int64ValuesStorage"/> collection.
        /// </summary>
        /// <remarks>This enumerator allows iteration over the values stored in an <see
        /// cref="Int64ValuesStorage"/> instance. It maintains the current position within the collection and provides
        /// access to the current element.</remarks>
        private sealed class Enumerator : IEnumerator<object?>
        {
            private Int32ValuesStorage storage;
            private int currentIndex = -1;

            public Enumerator(Int32ValuesStorage storage)
            {
                this.storage = storage;
                currentIndex = -1;
            }

            public object? Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= storage._intValues.Length)
                        throw new InvalidOperationException("Enumerator is not positioned within the collection.");
                    return storage.GetValue(currentIndex);
                }
            }

            object IEnumerator.Current => Current!;

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < storage.Count;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public void Dispose()
            {
                // No-op
            }
        }
    }
}
