using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Provides storage for nullable <see cref="DateTime"/> values using internal tick representation for native interop.
    /// </summary>
    internal sealed class DateTimeStorage : AbstractValueStorage, IEnumerable<object?>, IDisposable
    {
        private readonly long[] _ticks;
        private readonly NullBitMap _nullMap;
        private readonly GCHandle _handle;
        private bool _disposed;

        public DateTimeStorage(DateTime?[] values)
        {
            _ticks = new long[values.Length];
            _nullMap = new NullBitMap(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    _ticks[i] = values[i].Value.Ticks;
                    _nullMap.SetNull(i, false);
                }
                else
                {
                    _ticks[i] = 0; // placeholder
                    _nullMap.SetNull(i, true);
                }
            }

            _handle = GCHandle.Alloc(_ticks, GCHandleType.Pinned);
        }

        internal override int Count => _ticks.Length;

        internal override Type ElementType => typeof(DateTime);

        internal override object? GetValue(int index)
        {
            ValidateIndex(index);
            return _nullMap.IsNull(index) ? null : new DateTime(_ticks[index], DateTimeKind.Utc);
        }

        internal override void SetValue(int index, object? value)
        {
            ValidateIndex(index);

            if (value is null)
            {
                _nullMap.SetNull(index, true);
                _ticks[index] = 0;
                return;
            }

            if (value is DateTime dt)
            {
                _ticks[index] = dt.Ticks;
                _nullMap.SetNull(index, false);
                return;
            }

            if (value is IConvertible convertible)
            {
                try
                {
                    _ticks[index] = Convert.ToDateTime(convertible).Ticks;
                    _nullMap.SetNull(index, false);
                    return;
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Value must be convertible to DateTime.");
                }

            }

            throw new ArgumentException("Value must be of type DateTime or convertible to DateTime.");
        }

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < _ticks.Length; i++)
                {
                    if (_nullMap.IsNull(i))
                        yield return i;
                }
            }
        }

        internal override nint GetNativeBufferPointer()
        {
            EnsureNotDisposed();
            return _handle.AddrOfPinnedObject();
        }

        public long[] RawTicks
        {
            get
            {
                EnsureNotDisposed();
                return _ticks;
            }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            EnsureNotDisposed();
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _ticks.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DateTimeStorage));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle.IsAllocated)
                    _handle.Free();

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~DateTimeStorage()
        {
            Dispose();
        }

        private sealed class Enumerator : IEnumerator<object?>
        {
            private readonly DateTimeStorage storage;
            private int _currentIndex = -1;

            public Enumerator(DateTimeStorage storage)
            {
                this.storage = storage;
            }

            public object? Current
            {
                get
                {
                    if (_currentIndex < 0 || _currentIndex >= storage.Count)
                        throw new InvalidOperationException();
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
