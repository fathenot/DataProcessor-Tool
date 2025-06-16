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

        internal ObjectValueStorage(object?[] objects)
        {
            this.objects = objects.Select(o => UniversalDeepCloner.DeepClone(o)).ToArray();
        }

        internal override nint GetNativeBufferPointer()
        {
            throw new NotImplementedException();
        }

        internal override object? GetValue(int index)
        {
            return objects[index];
        }

        internal override void SetValue(int index, object? value)
        {
            objects[index] = value;
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

        internal override Type ElementType => typeof(object);

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
