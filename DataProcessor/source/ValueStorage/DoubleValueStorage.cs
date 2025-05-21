using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class DoubleValueStorage : ValueStorage
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

        public override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }
        public override object? GetValue(int index)
        {
            return array[index];
        }
        public override int Count => array.Length;
        public override IEnumerable<int> NullIndices
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

        public override void SetValue(int index, object? value)
        {
            if (value is double doubleValue)
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

        public override Type ElementType => typeof(double);
    }
}
