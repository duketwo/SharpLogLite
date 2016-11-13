
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;

/*
 * User: duketwo - https://github.com/duketwo/
 * Date: 10.11.2016
 */

namespace SharpLogLite.Utility
{
    /// <summary>
    /// Description of FixzedSizeQueue.
    /// </summary>
    public class FixedSizedQueue<T> : ConcurrentQueue<T>, INotifyCollectionChanged
    {
        private readonly object syncObject = new object();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public bool TryDequeue()
        {
            T item;
            bool b = base.TryDequeue(out item);
            if (b)
            {
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove, item));
            }
            return b;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, obj));

            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    this.TryDequeue(out outObj);
                }
            }
        }
    }
}
