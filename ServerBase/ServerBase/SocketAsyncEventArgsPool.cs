using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class SocketAsyncEventArgsPool
    {
        //just for assigning an ID so we can watch our objects while testing.
        private Int32 nextTokenId = 0;

        // Pool of reusable SocketAsyncEventArgs objects.
        private Queue<SocketAsyncEventArgs> pool;

        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        public SocketAsyncEventArgsPool(Int32 capacity)
        {
            pool = new Queue<SocketAsyncEventArgs>(capacity);
        }

        // The number of SocketAsyncEventArgs instances in the pool.
        public int Count
        {
            get { return pool.Count; }
        }

        // Removes a SocketAsyncEventArgs instance from the pool.
        // returns SocketAsyncEventArgs removed from the pool.
        public SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                if(0 == pool.Count)
                {
                    SocketAsyncEventArgs item = new SocketAsyncEventArgs();
                    pool.Enqueue(item);
                }

                return pool.Dequeue();
            }
        }

        // Add a SocketAsyncEventArg instance to the pool.
        // "item" = SocketAsyncEventArgs instance to add to the pool.
        public bool Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                //throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
                return false;
            }

            lock (pool)
            {
                pool.Enqueue(item);
            }
            return true;
        }
    }
}
