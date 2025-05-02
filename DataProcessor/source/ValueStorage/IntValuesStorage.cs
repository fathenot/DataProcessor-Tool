using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class IntStorage : ValueStorage
    {
        private readonly long?[] array;
        GCHandle handle;

        internal IntStorage(long?[] intValues)
        {
            this.array = intValues;
            handle = GCHandle.Alloc(array, GCHandleType.Pinned | GCHandleType.Pinned);
        }

        public override object? GetValue(int index)
        {
            return array.GetValue(index);
        }

        public override nint GetArrayAddress()
        {
            return handle.AddrOfPinnedObject();
        }

        public override int Length => array.Length;
        public override Type ValueType => typeof(int);

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
            if (index < 0 || index >= array.Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (value is int intValue)
            {
                array.SetValue(index, intValue);
            }
            if (value is null)
            {
                array[index] = null;
            }
        }
        ~IntStorage()
        {
            handle.Free();
        }
    }
}
