using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Represents a value storage specialized for storing nullable DateTime values.
    /// Internally uses ticks (long) for high performance interoperability with unmanaged code.
    /// </summary>
    internal class DateTimeStorage : ValueStorage
    {
        private readonly long[] _ticks;
        private readonly NullBitMap _nullMap;
        private readonly GCHandle _handle;

        /// <summary>
        /// Initializes a new instance of <see cref="DateTimeStorage"/> using a nullable DateTime array.
        /// </summary>
        /// <param name="dates">The array of nullable DateTime values to store.</param>
        internal DateTimeStorage(DateTime?[] dates)
        {
            _ticks = new long[dates.Length];
            _nullMap = new NullBitMap(dates.Length);

            for (int i = 0; i < dates.Length; i++)
            {
                if (dates[i].HasValue)
                {
                    _ticks[i] = dates[i].Value.Ticks;
                }
                else
                {
                    _nullMap.SetNull(i, true);
                    _ticks[i] = 0; // Placeholder for null
                }
            }

            _handle = GCHandle.Alloc(_ticks, GCHandleType.Pinned);
        }

        public override int Count => _ticks.Length;

        public override Type ElementType => typeof(DateTime);

        public override object? GetValue(int index)
        {
            ValidateIndex(index);
            return _nullMap.IsNull(index) ? null : new DateTime(_ticks[index], DateTimeKind.Utc);
        }

        public override void SetValue(int index, object? value)
        {
            ValidateIndex(index);

            if (value is null)
            {
                _nullMap.SetNull(index, true);
                _ticks[index] = 0;
            }
            else if (value is DateTime dt)
            {
                _ticks[index] = dt.Ticks;
                _nullMap.SetNull(index, false);
            }
            else
            {
                throw new InvalidCastException("Expected a DateTime or null.");
            }
        }

        public override IEnumerable<int> NullIndices
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

        public override nint GetNativeBufferPointer()
        {
            return _handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Gets the raw ticks array.
        /// </summary>
        public long[] RawTicks => _ticks;

        /// <summary>
        /// Frees the pinned handle. Must be called if you manually manage lifetime.
        /// </summary>
        public void DisposeHandle()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        ~DateTimeStorage()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }
    }
}
