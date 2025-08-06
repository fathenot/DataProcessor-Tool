using System.Collections;
namespace DataProcessor.source.ValueStorage
{
    /// <summary>
    /// Provides storage for an array of objects, allowing access to individual elements and their native buffer
    /// pointer.
    /// </summary>
    /// <remarks>This class is designed to manage an array of objects, offering functionality to retrieve and
    /// modify values, determine the count of elements, and identify indices of null values. It also provides access to
    /// the native memory buffer associated with the stored objects.</remarks>
    internal class ObjectValueStorage : AbstractValueStorage, IEnumerable<object?>
    {
        private readonly object?[] objects;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueStorage"/> class with the specified array of
        /// objects.
        /// </summary>
        /// <remarks>When <paramref name="copy"/> is <see langword="false"/>, the input array is stored
        /// directly without cloning. This can lead to unintended side effects if the array contains mutable objects
        /// that are modified after being passed to this constructor. Use this option with caution.</remarks>
        /// <param name="objects">An array of objects to be stored. The array can contain null values.</param>
        /// <param name="copy">A boolean value indicating whether the objects in the array should be deep-cloned. If <see
        /// langword="true"/>, each object in the array is deep-cloned before being stored. If <see langword="false"/>,
        /// the input array is stored as-is, which assumes the caller has ensured the array is safe to use.</param>
        internal ObjectValueStorage(object?[] objects, bool copy = true)
        {
            if(copy)
            {
                this.objects = objects.Select(o => UniversalDeepCloner.DeepClone(o)).ToArray();
            }
            else
            {
                // If not copying, we assume the input array is already cloned or does not require cloning.
                // This is a risky operation if the input array contains mutable objects.
                // Use with caution.
                this.objects = objects;
            }
        }

        internal override nint GetNativeBufferPointer()
        {
            throw new NotImplementedException();
        }
        internal override int Count => objects.Length;
        internal override IEnumerable<int> NullIndices
        {
            get
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null)
                    {
                        yield return i;
                    }
                }
            }
        }

        internal object[] Values => objects.Where(element => element is not null).ToArray();
        internal override Type ElementType => typeof(object);

        internal override object? GetValue(int index)
        {
            return objects[index];
        }

        internal override void SetValue(int index, object? value)
        {
            objects[index] = value;
        }

        public override IEnumerator<object?> GetEnumerator()
        {
            return new ObjectValueEnumerator(objects);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class ObjectValueEnumerator : IEnumerator<object?>
        {
            private readonly object?[] data;
            private int currentIndex = -1;

            public ObjectValueEnumerator(object?[] data)
            {
                this.data = data;
            }

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < data.Length;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public object? Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= data.Length)
                        throw new InvalidOperationException();
                    return data[currentIndex];
                }
            }

            object? IEnumerator.Current => this.Current;

            public void Dispose()
            {
                // No unmanaged resources to clean up in this case.
            }
        }
    }
}
