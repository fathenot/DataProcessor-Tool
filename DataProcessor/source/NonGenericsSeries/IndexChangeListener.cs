using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    internal interface IndexChangeNotifier
    {
        void RegisterView(IndexChangeListener indexChangeListener);
        void UnregisterView(IndexChangeListener indexChangeListener);
        void Notify();
    }
}
