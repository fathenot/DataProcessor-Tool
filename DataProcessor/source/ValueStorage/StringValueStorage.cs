using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class StringStorage : ValueStorage
    {
        private readonly string?[] strings;
        GCHandle handle;

        internal StringStorage(string?[] strings)
        {
            this.strings = strings;
            handle = GCHandle.Alloc(strings, GCHandleType.Pinned);
        }

        public string?[] Strings
        {
            get { return strings; }
        }
        public GCHandle Handle { get { return handle; } }

        public override nint GetArrayAddress()
        {
           return handle.AddrOfPinnedObject();
        }
        public override object? GetValue(int index)
        {
            return strings[index];
        }
        public override void SetValue(int index, object? value)
        {
            if(value == null)
            {
                strings[index] = null;
            }
            if(value is string stringValue)
            {
                strings[index] = stringValue;
            }
        }

        public override int Length => strings.Length;

        public override IEnumerable<int> NullPositions
        {
            get
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    if (strings[i] == null)
                    {
                        yield return i;

                    }
                }
            }
        }

        public override Type ValueType => typeof(string);
    }
}
