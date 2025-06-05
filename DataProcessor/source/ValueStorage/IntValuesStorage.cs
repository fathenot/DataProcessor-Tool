using DataProcessor.source.ValueStorage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DataProcessor.Source.ValueStorage
{
    /// <summary>
    /// Provides storage for nullable 64-bit integer values, with support for null tracking and native buffer access.
    /// </summary>
    internal class IntValuesStorage :AbstractValueStorage, IEnumerable<object?>
    {
        private readonly long[] _intValues;
        private readonly NullBitMap _bitMap;
        private readonly GCHandle _handle;

        public IntValuesStorage(long?[] values)
        {
            _intValues = new long[values.Length];
            _bitMap = new NullBitMap(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    _intValues[i] = values[i].Value;
                }
                _bitMap.SetNull(i, !values[i].HasValue);
            }

            _handle = GCHandle.Alloc(_intValues, GCHandleType.Pinned);
        }

        internal override object? GetValue(int index)
        {
            ValidateIndex(index);
            return _bitMap.IsNull(index) ? null : _intValues[index];
        }

        internal override void SetValue(int index, object? value)
        {
            ValidateIndex(index);

            if (value is null)
            {
                _bitMap.SetNull(index, true);
                _intValues[index] = default;
                return;
            }

            if (value is IConvertible convertible)
            {
                _intValues[index] = Convert.ToInt64(convertible);
                _bitMap.SetNull(index, false);
                return;
            }

            throw new InvalidCastException("Value must be a numeric type or null.");
        }

        internal override nint GetNativeBufferPointer() => _handle.AddrOfPinnedObject();

        internal override int Count => _intValues.Length;

        internal override Type ElementType => typeof(long);

        public int CountNullValues => _bitMap.CountNulls();

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < _intValues.Length; i++)
                {
                    if (_bitMap.IsNull(i))
                        yield return i;
                }
            }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        ~IntValuesStorage()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _intValues.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        private sealed class Enumerator : IEnumerator<object?>
        {
            private IntValuesStorage storage;
            private int _currentIndex = -1;

            public Enumerator(IntValuesStorage storage)
            {
               this.storage = storage;
                _currentIndex = -1;
            }

            public object? Current
            {
                get
                {
                    if (_currentIndex < 0 || _currentIndex >= storage._intValues.Length)
                        throw new InvalidOperationException("Enumerator is not positioned within the collection.");
                    return storage.GetValue(_currentIndex);
                }
            }

            object IEnumerator.Current => Current!;

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < storage.Count;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose()
            {
                // No-op
            }
        }
    }
}
