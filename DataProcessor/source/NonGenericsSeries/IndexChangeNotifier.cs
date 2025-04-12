using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    internal interface IndexChangeListener
    {
        void UpdateIndex();
    }
}
