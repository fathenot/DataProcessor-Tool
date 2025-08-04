using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class CharStorage : AbstractValueStorage, IEnumerable<object?>
    {
        char[] chars;
        NullBitMap nullbitMap;
        GCHandle handle;
        /// <summary>
        /// Initializes a new instance of the <see cref="CharStorage"/> class, storing an array of characters and tracking null
        /// values using a bitmap.
        /// </summary>
        /// <remarks>This constructor processes the input array to separate null values from actual character values. Null
        /// values are recorded in a bitmap for efficient tracking, while non-null values are converted to characters and stored
        /// in the internal array. The internal array is pinned in memory using a <see cref="GCHandle"/> to ensure it remains
        /// accessible during operations.</remarks>
        /// <param name="chars">An array of nullable characters to be stored. Null values are tracked separately and replaced with the default
        /// character value (<see langword="'\0'"/>).</param>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="CharStorage"/> class, which manages a collection of characters
        /// with optional copying and nullability tracking.
        /// </summary>
        /// <remarks>If <paramref name="copy"/> is <see langword="false"/>, the caller must ensure that
        /// the provided array is not modified externally while being managed by this instance. The class also
        /// initializes a nullability map for the characters, marking all characters as non-null by default.</remarks>
        /// <param name="chars">An array of characters to be managed by this instance. Cannot be null.</param>
        /// <param name="copy">A boolean value indicating whether the provided <paramref name="chars"/> array should be copied. If <see
        /// langword="true"/>, the array is copied; otherwise, the instance directly references the provided array.</param>
        internal CharStorage(char[] chars, bool copy = true)
        {
            if (copy)
            {
                this.chars = new char[chars.Length];
                Array.Copy(chars, this.chars, chars.Length);
            }
            else
            {
                this.chars = chars;
            }
            this.nullbitMap = new NullBitMap(chars.Length);
            for (int i = 0; i < chars.Length; i++)
            {
                nullbitMap.SetNull(i, false);
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
