using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class CharStorage : AbstractValueStorage, IEnumerable<object?>
    {
        char[] chars;
        NullBitMap nullbitMap;
        GCHandle handle;

        public CharStorage(char?[] chars)
        {
            this.chars = new char[chars.Length];
            nullbitMap = new NullBitMap(chars.Length);
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == null)
                {
                    nullbitMap.SetNull(i, true);
                    this.chars[i] = default;
                }
                else
                {
                    this.chars[i] = Convert.ToChar(chars[i]);
                    nullbitMap.SetNull(i, false);
                }
            }
            handle = GCHandle.Alloc(this.chars, GCHandleType.Pinned);
        }

        internal override Type ElementType => typeof(char);

        internal override int Count => chars.Length;

        internal override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }

        internal override object? GetValue(int index)
        {
            if (nullbitMap.IsNull(index))
            {
                return null;
            }
            return chars[index];
        }

        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (nullbitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }

        internal override void SetValue(int index, object? value)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for storage with count {Count}.");
            }
            if (value is char charValue)
            {
                chars[index] = charValue;
                nullbitMap.SetNull(index, false);
            }
            else if (value is null)
            {
                chars[index] = default;
                nullbitMap.SetNull(index, true);
            }
            else
            {
                throw new ArgumentException($"Expected a value of type {typeof(char)} or null.");
            }

        }


        private sealed class CharValueEnumerator : IEnumerator<object?>
        {
            /// <summary>
            /// this class make for creating enumerator
            /// </summary>

            private readonly CharStorage storage;
            private int currentIndex = -1;
            public CharValueEnumerator(CharStorage storage)
            {
                this.storage = storage;
            }
            public object? Current => storage.GetValue(currentIndex);
            object? System.Collections.IEnumerator.Current => Current;
            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < storage.Count;
            }
            public void Reset()
            {
                currentIndex = -1;
            }
            public void Dispose() { }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new CharValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CharValueEnumerator(this);
        }

        ~CharStorage()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }
}
