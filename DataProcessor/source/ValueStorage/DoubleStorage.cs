using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class DoubleStorage : ValueStorage
    {
        private readonly double?[] array;
        GCHandle handle;

        internal DoubleStorage(double?[] array)
        {
            this.array = array;
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        }

        public override nint GetArrayAddress()
        {
            return handle.AddrOfPinnedObject();
        }
        public override object? GetValue(int index)
        {
            return array[index];
        }
        public override int Length => array.Length;
        public override IEnumerable<int> NullPositions
        {
            get
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == null)
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
            }
            if (value is null)
            {
                array[index] = null;
            }
        }

        public override Type ValueType => typeof(double);
    }
}
