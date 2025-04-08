using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.Non_Generics_Series
{
   public partial class Series
    {
        // support method to check type is valid to add the data series
        private bool IsValidType(object? value)
        {
            return value == null
                 || value == DBNull.Value
                 || (value.GetType().IsValueType && dType.IsValueType && value.GetType() == dType)
                 || dType.IsAssignableFrom(value.GetType());
        }
    }
}
