using System.Collections;
using System.Text;

namespace DataProcessor.source.Index
{
    public class StringIndex : IIndex, IEnumerable<string>
    {
        private readonly string[] stringIndexes;
        private readonly Dictionary<string, List<int>> indexMap;

        /// <summary>
        /// Normalizes a Unicode string to the specified normalization form.
        /// </summary>
        /// <param name="input">The input string to normalize. This parameter cannot be <see langword="null"/>.</param>
        /// <param name="form">The normalization form to apply. The default is <see cref="NormalizationForm.FormC"/>, which performs
        /// canonical composition.</param>
        /// <returns>A new string that represents the normalized version of the input string in the specified normalization form.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <see langword="null"/>.</exception>
        private static string NormalizeUnicode(string input, NormalizationForm form = NormalizationForm.FormC)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Input cannot be null.");
            }
            // Normalize the string to Form C (Canonical Composition)
            return input.Normalize(form);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringIndex"/> class, which provides a mapping of strings to
        /// their indices in a normalized form for efficient lookups.
        /// </summary>
        /// <remarks>This constructor normalizes the input strings using the specified <paramref
        /// name="normalizationForm"/> and builds an internal mapping of normalized strings to their original indices.
        /// The mapping allows for efficient lookups of strings in their normalized form.</remarks>
        /// <param name="stringIndexes">A list of strings to be indexed. Each string will be normalized and mapped to its corresponding indices.</param>
        /// <param name="normalizationForm">The Unicode normalization form to apply to the strings. Defaults to <see cref="NormalizationForm.FormC"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stringIndexes"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="stringIndexes"/> contains <see langword="null"/> values.</exception>
        public StringIndex(List<string> stringIndexes, NormalizationForm normalizationForm = NormalizationForm.FormC)
        {
            // validate input
            if (stringIndexes == null)
            {
                throw new ArgumentNullException(nameof(stringIndexes), "String indexes cannot be null.");
            }
            if (stringIndexes.Contains(null))
            {
                throw new ArgumentException("String indexes cannot contain null values.");
            }

            this.stringIndexes = new string[stringIndexes.Count];
            this.indexMap = new Dictionary<string, List<int>>();
            for (int i = 0; i < stringIndexes.Count; i++)
            {
                var normalizedKey = NormalizeUnicode(stringIndexes[i], normalizationForm);
                this.stringIndexes[i] = normalizedKey;
                if (!indexMap.ContainsKey(normalizedKey))
                {
                    indexMap[normalizedKey] = new List<int>();
                }
                indexMap[normalizedKey].Add(i);
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public override int Count => stringIndexes.Length;

        /// <summary>
        /// Gets a read-only list of index objects.
        /// </summary>
        public override IReadOnlyList<object> IndexList => stringIndexes.AsReadOnly();

        /// <summary>
        /// Creates a new index by slicing the current index based on the specified range and step size.
        /// </summary>
        /// <param name="start">The starting index of the slice. Must be within the bounds of the current index.</param>
        /// <param name="end">The ending index of the slice. Must be within the bounds of the current index.</param>
        /// <param name="step">The step size for the slice. A positive value slices forward, and a negative value slices backward. Must not
        /// be 0.</param>
        /// <returns>A new <see cref="IIndex"/> containing the elements from the current index that match the specified slicing
        /// criteria.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is 0.</exception>
        public override IIndex Slice(int start, int end, int step = 1)
        {
            List<string> slicedIndex = new List<string>();

            if (step == 0)
            {
                throw new ArgumentException($"step must not be 0");
            }
            else if (step > 0)
            {
                for (int i = start; i <= end; i += step)
                {
                    slicedIndex.Add(stringIndexes[i]);
                }
            }
            else
            {
                for (int i = start; i >= end; i += step)
                {
                    slicedIndex.Add(stringIndexes[i]);
                }
            }
            return new StringIndex(slicedIndex);
        }

        public override IIndex Slice(List<object> indexList)
        {
            List<string> slicedIndex = new List<string>();
            foreach (var item in indexList)
            {
                if (item is string strItem && indexMap.ContainsKey(NormalizeUnicode(strItem)))
                {
                    foreach (var position in indexMap[NormalizeUnicode(strItem)])
                    {
                        slicedIndex.Add(stringIndexes[position]);
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid item {item} in index list.");
                }
            }
            return new StringIndex(slicedIndex);
        }

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection. Must be a string.</param>
        /// <returns><see langword="true"/> if the collection contains an element with the specified key;  otherwise, <see
        /// langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is not of type <see cref="string"/>.</exception>
        public override bool Contains(object key)
        {
            if (key is string strKey)
            {
                var tmp = NormalizeUnicode(strKey);
                return indexMap.ContainsKey(tmp);
            }
            throw new ArgumentException($"{nameof(key)} must be string.");
        }

        /// <summary>
        /// Finds the first position of the specified key in the index map.
        /// </summary>
        /// <param name="key">The key to search for. Must be a string.</param>
        /// <returns>The zero-based index of the first occurrence of the specified key in the index map.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is not a string.</exception>
        public override int FirstPositionOf(object key)
        {
            if (key is string strKey)
            {
                var tmp = NormalizeUnicode(strKey);
                return indexMap[tmp][0];
            }
            throw new ArgumentException($"{nameof(key)} must be string.");
        }

        /// <summary>
        /// Retrieves the object at the specified index from the collection.
        /// </summary>
        /// <param name="idx">The zero-based index of the object to retrieve.</param>
        /// <returns>The object at the specified index.</returns>
        public override object GetIndex(int idx)
        {
            return stringIndexes[idx];
        }

        /// <summary>
        /// Retrieves the list of index positions associated with the specified key.
        /// </summary>
        /// <param name="index">The key used to look up index positions. Must be a string.</param>
        /// <returns>A read-only list of integers representing the index positions associated with the specified key.</returns>
        public override IReadOnlyList<int> GetIndexPosition(object index)
        {
            return new List<int>(indexMap[NormalizeUnicode((string)index)]);
        }
        /// <summary>
        /// Returns a collection of distinct string indices.
        /// </summary>
        /// <remarks>The returned collection excludes duplicate indices and preserves only unique
        /// values.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of strings containing the distinct indices.</returns>
        public override IEnumerable<string> DistinctIndices()
        {
            return stringIndexes.Distinct();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of strings.
        /// </summary>
        /// <remarks>The enumerator provides read-only access to the collection and iterates in the order
        /// in which the strings are stored.</remarks>
        /// <returns>An enumerator that can be used to iterate through the strings in the collection.</returns>
        public override IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < stringIndexes.Length; i++)
            {
                yield return stringIndexes[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the value at the specified index in the collection.
        /// </summary>
        /// <remarks>The getter retrieves the value at the specified index, while the setter updates the
        /// value at the index and maintains an internal mapping of values to their indices. The index must be within
        /// the valid range of the collection.</remarks>
        /// <param name="index">The zero-based index of the value to get or set. Must be within the bounds of the collection.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the collection.</exception>
        public override object this[int index]
        {
            get
            {
                if (index < 0 || index >= stringIndexes.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return stringIndexes[index];
            }

            protected set
            {
                if (index < 0 || index >= stringIndexes.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                var oldValue = stringIndexes[index];
                stringIndexes[index] = value.ToString();

                // Cập nhật indexMap
                if (indexMap.ContainsKey(oldValue))
                {
                    indexMap[oldValue].Remove(index);
                    if (indexMap[oldValue].Count == 0)
                    {
                        indexMap.Remove(oldValue);
                    }
                }

                if (!indexMap.ContainsKey(Convert.ToString(value)))
                {
                    indexMap[Convert.ToString(value)] = new List<int>();
                }
                indexMap[Convert.ToString(value)].Add(index);
            }

        }

        public override IIndex Clone()
        {
            return new StringIndex(stringIndexes.ToList());
        }
    }
}
