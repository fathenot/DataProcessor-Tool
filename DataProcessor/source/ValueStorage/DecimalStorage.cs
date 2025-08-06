using System.Collections;
using System.Runtime.InteropServices;

namespace DataProcessor.source.ValueStorage
{
    internal class DecimalStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly decimal[] decimals;
        private NullBitMap nullBitMap;
        private GCHandle handle;

        /// <summary>
        /// Validates that the specified index is within the bounds of the <see cref="decimals"/> array.
        /// </summary>
        /// <param name="index">The index to validate. Must be greater than or equal to 0 and less than the length of the <see
        /// cref="decimals"/> array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the <see
        /// cref="decimals"/> array.</exception>
        private void ValidateIndex(int index)
        {
                       if (index < 0 || index >= decimals.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalStorage"/> class, storing a collection of  nullable
        /// decimal values and tracking their null states.
        /// </summary>
        /// <remarks>This constructor processes the input array to separate null values from non-null
        /// values.  Null values are represented in the internal storage as the default value for <see cref="decimal"/> 
        /// and are tracked using a <see cref="NullBitMap"/>. The input array is pinned in memory using  <see
        /// cref="GCHandle"/> to ensure it remains accessible during the lifetime of the storage.</remarks>
        /// <param name="decimals">An array of nullable decimal values to be stored. Null values are tracked using a null bitmap.</param>
        internal DecimalStorage(decimal?[] decimals)
        {
            this.decimals = new decimal[decimals.Length];
            nullBitMap = new NullBitMap(decimals.Length);
            handle = GCHandle.Alloc(decimals, GCHandleType.Pinned);
            for (int i = 0; i < decimals.Length; i++)
            {
                if (decimals[i] == null)
                {
                    nullBitMap.SetNull(i, true);
                    this.decimals[i] = default; // Set to default value for decimal
                }
                else
                {
                    this.decimals[i] = Convert.ToDecimal(decimals[i]);
                    nullBitMap.SetNull(i, false);
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalStorage"/> class, which manages an array of decimal
        /// values with optional copying and nullability tracking.
        /// </summary>
        /// <remarks>This class provides storage for decimal values with support for nullability tracking
        /// through an internal bitmap. The array is pinned in memory to prevent it from being moved by the garbage
        /// collector.</remarks>
        /// <param name="decimals">The array of decimal values to be managed. Cannot be null.</param>
        /// <param name="copy">A boolean value indicating whether to create a copy of the provided <paramref name="decimals"/> array. If
        /// <see langword="true"/>, the array is copied; otherwise, the provided array is used directly.</param>

        internal DecimalStorage(decimal[] decimals, bool copy = true)
        {
            if (copy)
            {
                this.decimals = new decimal[decimals.Length];
                Array.Copy(decimals, this.decimals, decimals.Length);
            }
            else
            {
                this.decimals = decimals;
            }
            nullBitMap = new NullBitMap(decimals.Length);
            for (int i = 0; i < decimals.Length; i++)
            {
                nullBitMap.SetNull(i, false); // Initially set all to not null
            }
            handle = GCHandle.Alloc(this.decimals, GCHandleType.Pinned);
        }

        internal override int Count => decimals.Length;
        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < decimals.Length; i++)
                {
                    if (nullBitMap.IsNull(i))
                    {
                        yield return i;
                    }
                }
            }
        }
        internal override Type ElementType => typeof(decimal);
        
        internal decimal[] Values
        {
            get
            {
                decimal[] result = new decimal[decimals.Length - NullIndices.Count()];
                int current_idx = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (!nullBitMap.IsNull(i))
                    {
                        result[current_idx] = decimals[i];
                        current_idx++;
                    }
                }
                return result;
            }
        }
        internal override nint GetNativeBufferPointer()
        {
            return handle.AddrOfPinnedObject();
        }
        internal override object? GetValue(int index)
        {
            return nullBitMap.IsNull(index) ? null : decimals[index];
        }
        internal override void SetValue(int index, object? value)
        {
            ValidateIndex(index);
            if (!(value is null) && !(value is decimal))
            {
                throw new ArgumentException("Value must be of type decimal or null.", nameof(value));
            }
            if (value is null)
            {
                nullBitMap.SetNull(index, true);
                decimals[index] = default; // Set to default value for decimal
            }
            else
            {
                nullBitMap.SetNull(index, false);
                decimals[index] = Convert.ToDecimal(value);
            }

        }
      
        public override IEnumerator<object?> GetEnumerator()
        {
            return new DecimalValueEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~DecimalStorage()
        {
            if (handle.IsAllocated)
            {
                handle.Free(); // Free the pinned handle to prevent memory leaks
            }
        }
        private sealed class DecimalValueEnumerator : IEnumerator<object?>
        {
            DecimalStorage storage;
            int position;
            public DecimalValueEnumerator(DecimalStorage storage)
            {
               this.storage = storage;
                position = -1;
            }
            public bool MoveNext()
            {
                position++;
                return position <storage.Count;
            }
            public void Reset()
            {
                position = -1;
            }
            public object? Current
            {
                get
                {
                    if (position >= storage.Count || position < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return storage.GetValue(position);

                }
            }
            object? System.Collections.IEnumerator.Current => Current;
            public void Dispose() { }
        }
    }
}
