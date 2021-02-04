using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PC2
{
    class SafeRefCountedHolder<T> where T : class
    {
        private T value;
        public int refCount;
        public SafeRefCountedHolder(T v)
        {
            value = v;
            refCount = 1;
        }

        public void AddRef()
        {
            if (refCount <= 0)
            {
                throw new InvalidOperationException();
            }
            Interlocked.Increment(ref refCount);
        }

        public void ReleaseRef()
        {
            if (refCount <= 0)
            {
                throw new InvalidOperationException();
            }

            if (Interlocked.Decrement(ref refCount) <= 0)
            {
                IDisposable disposable = value as IDisposable;
                value = null;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        public T Value
        {
            get
            {
                if (refCount == 0)
                    throw new InvalidOperationException();
                return value;
            }
        }
    }

    class SafeRefCountedHolderTest{
        public static int nThreads = 6;
        public static void AddRefTest(SafeRefCountedHolder<object> holder)
        {
            Thread[] threads = new Thread[nThreads];
            for(int i = 0; i < nThreads; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                  {
                      holder.AddRef();
                      Console.WriteLine("Thread " + threadId + " added ref current count "+holder.refCount);
                  });
                threads[i].Start();
            }

            for(int i = 0; i < nThreads; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("final count " + holder.refCount);
        }

        public static void ReleaseRefTest(SafeRefCountedHolder<object> holder)
        {
            Thread[] threads = new Thread[nThreads+1];
            for(int i = 0; i < nThreads+1; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                  {
                      holder.ReleaseRef();
                      Console.WriteLine("Thread " + threadId + " released ref current count " + holder.refCount);
                  });
                threads[i].Start();
            }

            for(int i = 0; i<nThreads+1;i++)
                threads[i].Join();

            Console.WriteLine("final count " + holder.refCount);
            try
            {
                holder.AddRef();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Invalid operation after dispose");
            }
        }

        public static void Main()
        {
            SafeRefCountedHolder<object> holder = new SafeRefCountedHolder<object>(new object());
            AddRefTest(holder);
            ReleaseRefTest(holder);
            Console.ReadKey();
        }
    }
}
    