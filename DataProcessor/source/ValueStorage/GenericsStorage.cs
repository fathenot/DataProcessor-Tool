using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Represents a generic, type-safe value storage implementation for the DataSharp engine.
    /// Stores values of type <typeparamref name="T"/> with null tracking using a bitmap.
    /// </summary>
    /// <typeparam name="T">The data type to store in this storage.</typeparam>
    internal class GenericsStorage<T> : AbstractValueStorage, IEnumerable<object?>, IDisposable
        where T: notnull
    {
        /// <summary>
        /// The array of values stored in this storage instance.
        /// </summary>
        private readonly T[] values;

        /// <summary>
        /// Bitmap used to track null values at each position.
        /// </summary>
        private readonly NullBitMap nullBitMap;

        /// <summary>
        /// GCHandle to pin the internal array in memory for native interop.
        /// </summary>
        private readonly GCHandle handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsStorage{T}"/> class from a list of values.
        /// Automatically detects and marks nulls in the null bitmap.
        /// </summary>
        /// <param name="values">The list of values to store.</param>
        internal GenericsStorage(List<T> values)
        {
            this.values = values.ToArray();
            nullBitMap = new NullBitMap(values.Count);

            for (int i = 0; i < values.Count; i++)
            {
                nullBitMap.SetNull(i, values[i] == null);
            }

            handle = GCHandle.Alloc(this.values, GCHandleType.Pinned);
        }

        /// <inheritdoc />
        internal override int Count => values.Length;

        /// <inheritdoc />
        internal override Type ElementType => typeof(T);

        /// <inheritdoc />
        internal override object? GetValue(int index)
        {
            return values[index];
        }

        /// <inheritdoc />
        internal override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }

        /// <inheritdoc />
        internal override void SetValue(int index, object? value)
        {
            if (index < 0 || index >= values.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (value is T typedValue)
            {
                values[index] = typedValue;
                nullBitMap.SetNull(index, false);
            }
            else if (value is null)
            {
                values[index] = default!;
                nullBitMap.SetNull(index, true);
            }
            else
            {
                throw new InvalidCastException($"Expected a value of type {typeof(T)} or null.");
            }
        }

        /// <summary>
        /// Gets the indexes of the null values in this storage.
        /// </summary>
        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (nullBitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }

        private sealed class GenericsValueEnumerator : IEnumerator<object?>
        {
            private readonly T[] values;
            private readonly NullBitMap nullBitMap;
            private int currentIndex = -1;

            public GenericsValueEnumerator(T[] values, NullBitMap nullBitMap)
            {
                this.values = values;
                this.nullBitMap = nullBitMap;
            }

            public object? Current => nullBitMap.IsNull(currentIndex) ? null : values[currentIndex];

            object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < values.Length;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public void Dispose() { }
        }

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new GenericsValueEnumerator(values, nullBitMap);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~GenericsStorage()
        {
           if(handle.IsAllocated)
           {
               handle.Free();
            }
        }
    }
}
