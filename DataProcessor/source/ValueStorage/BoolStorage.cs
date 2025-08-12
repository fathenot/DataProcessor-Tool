using DataProcessor.source.ValueStorage;
using System.Collections;

namespace DataProcessor.source.ValueStorage
{
    internal class BoolStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly BitArray _values;
        private readonly NullBitMap _nullBitMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolStorage"/> class 
        /// using the specified array of nullable Boolean values.
        /// </summary>
        /// <param name="bools">
        /// An array of nullable Boolean values to initialize the storage. 
        /// Each element represents a Boolean value or a null entry.
        /// </param>
        internal BoolStorage(bool?[] bools)
        {
            var length = bools.Length;
            _values = new BitArray(length);
            _nullBitMap = new NullBitMap(length);

            for (var i = 0; i < length; i++)
            {
                if (bools[i].HasValue)
                {
                    _values[i] = bools[i].Value;
                }
                else
                {
                    _values[i] = false; // Placeholder for nulls
                    _nullBitMap.SetNull(i, true);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolStorage"/> class 
        /// using the specified array of Boolean values.
        /// </summary>
        /// <param name="bools">
        /// The array of Boolean values to initialize the storage with.
        /// </param>
        internal BoolStorage(bool[] bools)
        {
            _values = new BitArray(bools);
            _nullBitMap = new NullBitMap(bools.Length);
        }

        internal override int Count => _values.Count;

        internal override Type ElementType => typeof(bool);

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < _values.Length; i++)
                {
                    if (_nullBitMap.IsNull(i))
                        yield return i;
                }
            }
        }

        /// <summary>
        /// Gets all non-null values from the storage.
        /// </summary>
        internal bool[] NonNullValues
        {
            get
            {
                var result = new bool[_values.Length - _nullBitMap.CountNulls()];
                var currentIndex = 0;

                for (var i = 0; i < Count; i++)
                {
                    if (!_nullBitMap.IsNull(i))
                    {
                        result[currentIndex] = _values[i];
                        currentIndex++;
                    }
                }

                return result;
            }
        }

        internal override nint GetNativeBufferPointer()
        {
            throw new NotImplementedException();
        }

        internal override object? GetValue(int index)
        {
            return _values[index];
        }

        internal override void SetValue(int index, object? value)
        {
            if (value == null)
            {
                _nullBitMap.SetNull(index, true);
                return;
            }

            if (value is bool b)
            {
                _values[index] = b;
                return;
            }

            throw new ArgumentException("Value must be a boolean or null.", nameof(value));
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            for (var i = 0; i < _values.Length; i++)
            {
                yield return _nullBitMap.IsNull(i) ? null : _values[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }
}
