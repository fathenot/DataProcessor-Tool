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

        public override nint GetNativeBufferPointer()
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

        public override int Count => strings.Length;

        public override IEnumerable<int> NullIndices
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

        public override Type ElementType => typeof(string);
    }
}
