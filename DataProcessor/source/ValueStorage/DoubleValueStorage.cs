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
            if (value is double || value is float)
            {
                array[index] = (double)value;
                nullBitMap.SetNull(index, false);
            }
            if (value is null)
            {
                array[index] = default;
                nullBitMap.SetNull(index, true);
            }
            else
            {
                throw new ArgumentException($"value: {nameof(value)} is not double");
            }
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
