using DataProcessor.source.ValueStorage;
using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class CharStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly char[] _chars;
        private readonly NullBitMap _nullBitMap;
        private readonly GCHandle _handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharStorage"/> class 
        /// with nullable character values and null tracking.
        /// </summary>
        /// <param name="chars">An array of nullable characters. Nulls are tracked separately.</param>
        public CharStorage(char?[] chars)
        {
            _chars = new char[chars.Length];
            _nullBitMap = new NullBitMap(chars.Length);

            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == null)
                {
                    _nullBitMap.SetNull(i, true);
                    _chars[i] = default;
                }
                else
                {
                    _chars[i] = chars[i].Value;
                    _nullBitMap.SetNull(i, false);
                }
            }

            _handle = GCHandle.Alloc(_chars, GCHandleType.Pinned);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharStorage"/> class 
        /// with character values and optional copying.
        /// </summary>
        /// <param name="chars">An array of characters.</param>
        /// <param name="copy">
        /// Whether to copy the array. If false, the provided array is used directly.
        /// </param>
        internal CharStorage(char[] chars, bool copy = true)
        {
            _chars = copy ? (char[])chars.Clone() : chars;
            _nullBitMap = new NullBitMap(chars.Length);

            for (var i = 0; i < chars.Length; i++)
            {
                _nullBitMap.SetNull(i, false);
            }

            _handle = GCHandle.Alloc(_chars, GCHandleType.Pinned);
        }

        internal override Type ElementType => typeof(char);

        internal override int Count => _chars.Length;

        internal char[] NonNullValues
        {
            get
            {
                var nonNullCount = Count - _nullBitMap.CountNulls();
                var result = new char[nonNullCount];

                var currentIndex = 0;
                for (var i = 0; i < Count; i++)
                {
                    if (!_nullBitMap.IsNull(i))
                    {
                        result[currentIndex++] = _chars[i];
                    }
                }

                return result;
            }
        }

        internal override nint GetNativeBufferPointer()
        {
            return _handle.AddrOfPinnedObject();
        }

        internal override object? GetValue(int index)
        {
            return _nullBitMap.IsNull(index) ? null : _chars[index];
        }

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (var i = 0; i < _chars.Length; i++)
                {
                    if (_nullBitMap.IsNull(i))
                        yield return i;
                }
            }
        }

        internal override StorageKind storageKind => StorageKind.Char;
        internal override void SetValue(int index, object? value)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range for storage with count {Count}.");
            }

            if (value is char charValue)
            {
                _chars[index] = charValue;
                _nullBitMap.SetNull(index, false);
            }
            else if (value is null)
            {
                _chars[index] = default;
                _nullBitMap.SetNull(index, true);
            }
            else
            {
                throw new ArgumentException(
                    $"Expected a value of type {typeof(char)} or null.",
                    nameof(value));
            }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new CharValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class CharValueEnumerator : IEnumerator<object?>
        {
            private readonly CharStorage _storage;
            private int _currentIndex = -1;

            public CharValueEnumerator(CharStorage storage)
            {
                _storage = storage;
            }

            public object? Current => _storage.GetValue(_currentIndex);

            object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _storage.Count;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose() { }
        }

        ~CharStorage()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }
    }
}
