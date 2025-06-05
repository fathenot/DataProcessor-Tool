using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class DecimalStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly decimal[] decimals;
        NullBitMap nullBitMap;
        GCHandle handle;
        internal DecimalStorage(decimal?[] decimals)
        {
            this.decimals = new decimal[decimals.Length];
            nullBitMap = new NullBitMap(decimals.Length);
            handle = GCHandle.Alloc(decimals, GCHandleType.Pinned);
            for (int i = 0; i < decimals.Length; i++)
            {
                if (decimals[i] == null)
                {
                    nullBitMap.SetNull(i, true);
                    this.decimals[i] = default; // Set to default value for decimal
                }
                else
                {
                    this.decimals[i] = Convert.ToDecimal(decimals[i]);
                    nullBitMap.SetNull(i, false);
                }
            }
        }
        internal override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }
        internal override object? GetValue(int index)
        {
            return nullBitMap.IsNull(index) ? null : decimals[index];
        }
        internal override void SetValue(int index, object? value)
        {
            if (value is decimal decimalValue)
            {
                decimals[index] = decimalValue;
            }
            else
            {
                nullBitMap.SetNull(index, true);
                decimals[index] = default; // Reset to default value if null
            }
        }
        internal override int Count => decimals.Length;
        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < decimals.Length; i++)
                {
                    if (nullBitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }
        internal override Type ElementType => typeof(decimal);

        public override IEnumerator<object?> GetEnumerator()
        {
            return new DecimalValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class DecimalValueEnumerator : IEnumerator<object?>
        {
            DecimalStorage storage;
            int position;
            public DecimalValueEnumerator(DecimalStorage storage)
            {
               this.storage = storage;
                position = -1;
            }
            public bool MoveNext()
            {
                position++;
                return position <storage.Count;
            }
            public void Reset()
            {
                position = -1;
            }
            public object? Current
            {
                get
                {
                    if (position >= storage.Count || position < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return storage.GetValue(position);

                }
            }
            object? System.Collections.IEnumerator.Current => Current;
            public void Dispose() { }
        }
    }
}
