
using DataProcessor.source.Index;
using NUnit.Framework;

namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        /// <summary>
        /// Creates an index based on the provided list of objects.
        /// </summary>
        /// <remarks>The method determines the appropriate index type by inferring the data type of the
        /// elements in the list. If the list contains grouped index elements, they are flattened and converted into a
        /// <see cref="MultiIndex"/>. For other cases, the method uses the inferred data type to create a specific index
        /// type.</remarks>
        /// <param name="index">A list of objects to be used for creating the index. The list must not contain null values.</param>
        /// <returns>An implementation of <see cref="IIndex"/> that corresponds to the data type of the elements in the provided
        /// list. If the list contains grouped index elements, a <see cref="MultiIndex"/> is returned. Otherwise, the
        /// method returns a specific index type such as <see cref="StringIndex"/>, <see cref="Int64Index"/>, <see
        /// cref="DateTimeIndex"/>, <see cref="DoubleIndex"/>, <see cref="CharIndex"/>, <see cref="DecimalIndex"/>, or
        /// <see cref="ObjectIndex"/> based on the inferred data type of the list elements.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> contains null values.</exception>
        internal static IIndex CreateIndex(List<object> index = null)
        {
            // validate the index to ensure it does not contains null values (currently it doesn't check all elements are hashable)
            if (index.Contains(null)) throw new ArgumentException("index must not contain nulls");

            if (index == null || index.Count == 0) throw new ArgumentException("index must not be null or empty", nameof(index));

            // check if the index contains grouped elements
            if (IndexUtils.ContainsGroupedIndex(index))
            {
                List<MultiKey> grouped = new List<MultiKey>();
                foreach (var item in index)
                {
                    var temp = IndexUtils.FlattenIndexElement(item).ToArray();
                    grouped.Add(new MultiKey(temp));
                }
                return new MultiIndex(grouped);
            }

            // if the index does not contain grouped elements, infer the data type and create the appropriate index
            var datatype = TypeInference.InferDataType(index);
            if (datatype == typeof(string))
            {
                return new StringIndex(index.Cast<string>().ToList());
            }
            else if (TypeInference.IsIntegerType(datatype))
            {
                return new Int64Index(index.Select(Convert.ToInt64).ToList());
            }
            else if (datatype == typeof(DateTime))
            {
                return new DateTimeIndex(index.Select(Convert.ToDateTime).ToList());
            }
            else if (TypeInference.IsFloatingType(datatype))
            {
                return new DoubleIndex(index.Select(Convert.ToDouble).ToList());
            }
            else if (datatype == typeof(char))
                return new CharIndex(index.Select(Convert.ToChar).ToList());
            else if (datatype == typeof(decimal))
                return new DecimalIndex(index.Select(Convert.ToDecimal).ToList());

            // if the datatype is not rcognized, return ObjectIndex
            return new ObjectIndex(index);
        }
    }
}
