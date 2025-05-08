using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.source.NonGenericsSeries
{
    internal class IndexSynchronizer: IndexChangeNotifier
    {
        private LinkedList<WeakReference<IndexChangeListener> > notifiers = new LinkedList<WeakReference<IndexChangeListener>> ();

        private void CleanDeadListeners()
        {
            var node = notifiers.First;
            while (node != null)
            {
                var next = node.Next;
                if (!node.Value.TryGetTarget(out _))
                {
                    notifiers.Remove(node);
                }
                node = next;
            }
        }

        public void RegisterView(IndexChangeListener listener)
        {
            foreach (var weakRef in notifiers)
            {
                if (weakRef.TryGetTarget(out var target) && ReferenceEquals(target, listener))
                {
                    throw new ArgumentException($"Listener already registered: {nameof(listener)}");
                }
            }

            notifiers.AddLast(new WeakReference<IndexChangeListener>(listener));
        }

        public void UnregisterView(IndexChangeListener listener)
        {
            var node = notifiers.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.TryGetTarget(out var target) && ReferenceEquals(target, listener))
                {
                    notifiers.Remove(node);
                    break;
                }
                node = next;
            }
        }

        public void Notify()
        {
            CleanDeadListeners();
            var node = this.notifiers.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.TryGetTarget(out var listener))
                {
                    listener.UpdateIndex();
                }
                node = next;
            }
        }
    }
}
