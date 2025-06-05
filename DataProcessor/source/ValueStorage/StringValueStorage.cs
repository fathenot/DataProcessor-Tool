using System.Runtime.InteropServices;
using System;
using System.Collections;
namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Provides storage for an array of nullable strings, with functionality to manage and access the data.
    /// </summary>
    /// <remarks>This class allows for efficient storage and manipulation of string values, including handling
    /// null values. It provides methods to retrieve and set values by index, access the underlying native buffer
    /// pointer, and  enumerate indices of null values. The storage is pinned in memory to ensure compatibility with
    /// native operations.</remarks>
    internal class StringStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly string?[] strings;
        GCHandle handle;


        // this is constructor
        internal StringStorage(string?[] strings)
        {
            this.strings = strings;
            handle = GCHandle.Alloc(strings, GCHandleType.Pinned);
        }

        /// <summary>
        /// Gets an array of strings.
        /// </summary>
        internal string?[] Strings
        {
            get { return strings; }
        }

        
        internal override Type ElementType => typeof(string);

        internal override nint GetNativeBufferPointer()
        {
           return handle.AddrOfPinnedObject();
        }
        internal override object? GetValue(int index)
        {
            return strings[index];
        }
        internal override void SetValue(int index, object? value)
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

        internal override int Count => strings.Length;

        internal override IEnumerable<int> NullIndices
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

        public override IEnumerator<object?> GetEnumerator()
        {
            return new StringValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~StringStorage()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

         
        private sealed class StringValueEnumerator : IEnumerator<object?>
        {
            private readonly StringStorage storage;
            private int currentIndex = -1;
            public StringValueEnumerator(StringStorage storage)
            {
                this.storage = storage;
            }
            public object? Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= storage.Count)
                    {
                        throw new InvalidOperationException("Enumerator is not positioned within the collection.");
                    }
                    return storage.GetValue(currentIndex);
                }
            }
            object? IEnumerator.Current => Current;
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
