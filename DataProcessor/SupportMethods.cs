using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    // this is class contains methods only used by this library developers 
    static internal class SupportMethods
    {
        internal static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        internal static void CheckNull(object? value)
        {
            if(value == null)
            {
                throw new Exception($"{nameof(value)} is null");
            }
        }
    }
}
