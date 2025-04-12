using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    internal class IndexSynchronizer: IndexChangeNotifier
    {
        LinkedList<IndexChangeListener> notifiers = new LinkedList<IndexChangeListener> ();
        public void RegisterView(IndexChangeListener listener)
        {
            notifiers.AddLast(listener);
        }
        public void UnregisterView(IndexChangeListener listener)
        {
            notifiers.Remove(listener);
        }
        public void Notify()
        {
            foreach (IndexChangeListener listener in notifiers)
            {
                listener.UpdateIndex();
            }
        }
    }
}
