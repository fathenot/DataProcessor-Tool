using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.ValueStorage;
using DataProcessor.source.NonGenericsSeries;
namespace DataProcessor.source.NonGenericsSeries
{
    public partial class Series
    {
        internal static AbstractValueStorage ValueStorageCreate(List<object?> elements, bool copy = false)
        {
            var dataType = Support.InferDataType(elements);
            if(dataType == typeof(bool))
            {
                // If the data type is boolean, we can use BoolStorage
                return new BoolStorage(elements.Select(e => e != null && Convert.ToBoolean(e)).ToArray());
            }
            if (dataType == typeof(object))
            {
                return new ObjectValueStorage(elements.ToArray(), copy);
            }
            else if (dataType == typeof(string))
            {
                return new StringStorage(elements.Select(Convert.ToString).ToArray(), copy);
            }
            else if (dataType == typeof(double))
            {
                return new DoubleValueStorage(elements.Select(Convert.ToDouble).ToArray(), copy);
            }
            else if (dataType == typeof(decimal))
            {
                return new DecimalStorage(elements.Select(Convert.ToDecimal).ToArray(), copy);
            }
            else if (dataType == typeof(DateTime))
            {
                return new DateTimeStorage(elements.Select(Convert.ToDateTime).ToArray(), copy);
            }
            else if (dataType == typeof(char))
            {
                return new CharStorage(elements.Select(Convert.ToChar).ToArray(), copy);
            }
            else if (Support.IsIntegerType(dataType))
            {
                if(elements.Contains(null))
                {
                    // If there are nulls, we need to use nullable long
                    var elementsWithNulls = elements.Select(e => e == null ? (long?)null : Convert.ToInt64(e)).ToArray();
                    return new IntValuesStorage(elementsWithNulls);
                }
                return new IntValuesStorage(elements.Select(Convert.ToInt64).ToArray(), copy);
            }
            return new ObjectValueStorage(elements.ToArray());

        }
    }
}
