using System.Collections;
using System.Text;
using DataProcessor.source.UserSettings;
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

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= strings.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
        }

        // this is constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="StringStorage"/> class with the specified array of strings.
        /// </summary>
        /// <remarks>If <paramref name="copy"/> is <see langword="false"/>, the input array is directly
        /// referenced, and any modifications to the input array after the constructor call will affect the stored data.
        /// If <paramref name="copy"/> is <see langword="true"/>, the input array is not modified. The normalization
        /// behavior is determined by the <see cref="UserSettings.NormalizeUnicode"/> property and the normalization
        /// form specified in <see cref="UserSettings.DefaultNormalizationForm"/>.</remarks>
        /// <param name="strings">An array of strings to be stored. The array can contain null values.</param>
        /// <param name="copy">A boolean value indicating whether to create a copy of the input array. If <see langword="true"/>, a new
        /// array is created and populated with the normalized or original strings. If <see langword="false"/>, the
        /// input array is used directly, and its elements are normalized in place if normalization is enabled.</param>
        internal StringStorage(string?[] strings, bool copy = true)
        {
            if(!copy)
            {
                this.strings = strings;
                for (int i = 0; i < strings.Length; i++)
                {
                    this.strings[i] = UserSettings.UserConfig.NormalizeUnicode ? strings[i]?.Normalize(UserSettings.UserConfig.DefaultNormalizationForm) : strings[i];
                }
            }
            else
            {
                this.strings = new string?[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    string? s = strings[i];
                    this.strings[i] = UserSettings.UserConfig.NormalizeUnicode ? s?.Normalize(UserSettings.UserConfig.DefaultNormalizationForm) : s;
                }
            }
           
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
            throw new NotSupportedException("StringStorage does not support native buffer pointer access.");
        }
        internal override object? GetValue(int index)
        {
            ValidateIndex(index);
            return strings[index];
        }
        internal override void SetValue(int index, object? value)
        {
            if (index < 0 || index >= strings.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            if (value != null && !(value is string))
            {
                throw new ArgumentException("Value must be a string or null.", nameof(value));
            }
            strings[index] = ((string?)value)?.Normalize(NormalizationForm.FormC);
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
