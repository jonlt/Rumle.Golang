using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rumle.Golang
{
    /// <summary>
    /// Golang-like channel
    /// </summary>
    /// <typeparam name="T">the type to use in the channel</typeparam>
    public class Channel<T>
    {
        private readonly int _bufferSize;
        private readonly Queue<T> _inner = new Queue<T>();

        public Channel(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public Channel()
        {
            _bufferSize = 0;
        }

        /// <summary>
        /// Send a value to the channel
        /// </summary>
        public void Send(T item)
        {
            int currentCount = 0;
            lock (_inner)
            {
                _inner.Enqueue(item);
                currentCount = _inner.Count;
            }
            SpinWait.SpinUntil(() => _inner.Count == currentCount - 1 || _inner.Count < _bufferSize + 1);
        }

        /// <summary>
        /// Get a value from the channel
        /// </summary>
        public T Receive()
        {
            T item = default(T);
            SpinWait.SpinUntil(() => TryReceive(out item));
            return item;
        }

        protected bool TryReceive(out T item)
        {
            if (Monitor.TryEnter(_inner))
            {
                if (_inner.Any())
                {
                    item = _inner.Dequeue();
                    Monitor.Exit(_inner);
                    return true;
                }
                Monitor.Exit(_inner);
            }
            item = default(T);
            return false;
        }

        /// <summary>
        /// This method is used be <see cref="Select"/> to handle channel receives. It is internal because it returns a lock that needs to be released at some point.
        /// </summary>
        /// <param name="lock">the lock for this chanel, if you received this, you have the responsibility to release it</param>
        internal bool TryEnterExclusive(out object @lock)
        {
            if (Monitor.TryEnter(_inner))
            {
                @lock = _inner;
                if (_inner.Any())
                {
                    return true;
                }
                else
                {
                    @lock = null;
                    Monitor.Exit(_inner);
                }
            }
            @lock = null;
            return false;
        }
    }
}
