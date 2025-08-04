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
        private readonly List<DateTimeKind> _kinds;
        private readonly NullBitMap _nullMap;
        private readonly GCHandle _handle;
        private bool _disposed;

        internal DateTimeStorage(DateTime?[] values)
        {
            _ticks = new long[values.Length];
            _nullMap = new NullBitMap(values.Length);
            this._kinds = new List<DateTimeKind>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    _ticks[i] = values[i].Value.Ticks;
                    _nullMap.SetNull(i, false);
                    _kinds.Add(values[i].Value.Kind);
                }
                else
                {
                    _ticks[i] = 0; // placeholder
                    _nullMap.SetNull(i, true);
                    _kinds.Add(DateTimeKind.Unspecified); // Default for null values
                }
            }

            _handle = GCHandle.Alloc(_ticks, GCHandleType.Pinned);
        }

        internal DateTimeStorage(DateTime[] dateTimes)
        {

            _ticks = dateTimes.Select(dt => dt.Ticks).ToArray();
            _kinds = dateTimes.Select(dt => dt.Kind).ToList();
            _nullMap = new NullBitMap(dateTimes.Length);
            for (int i = 0; i < dateTimes.Length; i++)
            {
                _nullMap.SetNull(i, false);
            }
            _handle = GCHandle.Alloc(_ticks, GCHandleType.Pinned);
        }

        internal DateTimeStorage(long[] ticks, List<DateTimeKind> kinds, bool copy = true)
        {
            // ck if ticks and kinds have the same length
            if (ticks.Length != kinds.Count)
                throw new ArgumentException("Ticks and kinds must have the same length.");

            if (copy)
            {
                _ticks = new long[ticks.Length];
                Array.Copy(ticks, _ticks, ticks.Length);
                _kinds = new List<DateTimeKind>(kinds);
            }
            else
            {
                _ticks = ticks;
                _kinds = kinds;
            }
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

            if (value is string s)
            {
                if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                {
                    _ticks[index] = parsed.Ticks;
                    _nullMap.SetNull(index, false);
                    return;
                }

                throw new ArgumentException("String value must be a valid ISO 8601 DateTime.");
            }

            if (value is IConvertible convertible)
            {
                try
                {
                    _ticks[index] = Convert.ToDateTime(convertible, System.Globalization.CultureInfo.InvariantCulture).Ticks;
                    _nullMap.SetNull(index, false);
                    return;
                }
                catch
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

        /// <summary>
        /// Gets an array of non-null <see cref="DateTime"/> values stored in this instance.
        /// </summary>
        /// <remarks>
        /// This property iterates through the internal storage, excluding any entries marked as null by the
        /// <c>_nullMap</c>. For each non-null element, it reconstructs the <see cref="DateTime"/> from its corresponding
        /// tick value in the <c>_ticks</c> array. The resulting array contains only valid <see cref="DateTime"/> values,
        /// and its length equals <c>Count - NullCount</c>.
        /// </remarks>
        /// <returns>
        /// An array of <see cref="DateTime"/> values representing the non-null elements in the storage.
        /// </returns>
        internal DateTime[] Values
        {
            get
            {
                var values = new DateTime[this.Count - this.NullIndices.Count()];
                int resultIdx = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (!this._nullMap.IsNull(i))
                    {
                        values[resultIdx] = new DateTime(_ticks[i]);
                        resultIdx++;
                    }
                }
                return values;
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
