using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class CharStorage : ValueStorage
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
            if(nullbitMap.IsNull(index))
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
                throw new InvalidCastException($"Expected a value of type {typeof(char)} or null.");
            }

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
