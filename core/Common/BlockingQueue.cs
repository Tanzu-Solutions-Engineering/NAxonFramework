using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NAxonFramework.EventHandling;

namespace NAxonFramework.Common
{
    public class BlockingQueue<T> : IEnumerable<T>
    {
        private int _count = 0;
        private Queue<T> _queue = new Queue<T>();

        public T Dequeue()
        {
            lock (_queue)
            {
                while (_count <= 0) Monitor.Wait(_queue);
                _count--;
                return _queue.Dequeue();
            }
        }
        
        


        public void Enqueue(T data)
        {
            if (data == null) throw new ArgumentNullException("data");
            lock (_queue)
            {
                _queue.Enqueue(data);
                _count++;
                Monitor.Pulse(_queue);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            while (true) yield return Dequeue();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        public T Poll(TimeSpan timeout)
        {
            lock (_queue)
            {
                bool isTimeout = false;
                if (_count <= 0)
                {
                    isTimeout = !Monitor.Wait(_queue, timeout);
                }
                _count--;
                return !isTimeout ? _queue.Dequeue() : default(T);
            }
        }
    }

}