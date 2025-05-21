using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class IntStorage : ValueStorage
    {
        private readonly long[] array;
        NullBitMap bitMap;
        GCHandle handle;

        internal IntStorage(long?[] intValues)
        {
            array = new long[intValues.Length];
            bitMap = new NullBitMap(intValues.Length);

            for (int i = 0; i < intValues.Length; i++)
            {
                bitMap.SetNull(i, intValues[i] == null);

                if (intValues[i] != null)
                {
                    array[i] = (long)intValues[i];
                }
            }
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        }

        public override object? GetValue(int index)
        {
            return bitMap.IsNull(index) ? null : array[index];
        }

        public override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }

        public override int Count => array.Length;
        public override Type ElementType => typeof(long);
        public int CountNullValues => bitMap.CountNulls();

        public override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (bitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }


        public override void SetValue(int index, object? value)
        {
            if (index < 0 || index >= array.Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (value is long longValue)
            {
                array[index] = longValue;
                bitMap.SetNull(index, false);
            }
            if (value is null)
            {
                bitMap.SetNull(index, true);
                array[index] = default;
            }
            else
            {
                throw new InvalidCastException("Expected a long or null");
            }
        }
        ~IntStorage()
        {
            handle.Free();
        }
    }
}
