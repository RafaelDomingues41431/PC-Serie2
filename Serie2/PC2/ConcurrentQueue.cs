using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PC2
{  
    public class ConcurrentQueue<T>
    {

        Node<T> dummy;
        Node<T> head;
        Node<T> tail;

        public string TestGetQueue()
        {
            string values="";
            Node<T> iterator = dummy.next;
            while (iterator != null)
            {
                values = values + " , " + iterator.value;
                iterator = iterator.next;
            }
            return values;
        }
        
        public ConcurrentQueue()
        {
            dummy = new Node<T>();
            head = dummy;
            tail = dummy;
        }

        public class Node<T>
        {
            public T value;
            public Node<T> next;
            public Node() { }
            public Node(T value, Node<T> next)
            {
                this.value = value;
                this.next = next;
            }
        }

        private class Queue<T>
        {
            public volatile Node<T> head;
            public volatile Node<T> tail;
        }

        private Queue<T> queue;
        
        public void Put(T item)
        {
            Node<T> newNode = new Node<T>(item, null);
            
            while (true)
            {
                Node<T> currTail = tail;
                Node<T> tailNext = tail.next;

                if (currTail == tail)
                {
                    if (tailNext != null)
                        Interlocked.CompareExchange(ref tail,tailNext, currTail);
                    else
                    {
                        if(Interlocked.CompareExchange(ref currTail.next, newNode, null) == null)
                        {
                            Interlocked.CompareExchange(ref tail, newNode, currTail);
                            return;
                        }
                    }
                }                 
            }
        }

        public T TryTake()
        {
            while(true){
                if (IsEmpty())
                    return default(T);
                Node<T> deleted;
                deleted = dummy.next;
                return Interlocked.CompareExchange(ref dummy.next, deleted.next, deleted).value;             
            }
        }

        public bool IsEmpty()
        {
            if (dummy.next == null)
                return true;
            else
                return false;
        }
    }

    public class TestConcurrentQueue
    {
        static int nThreads = 6;
        

        public static void PutThreadedTest(ConcurrentQueue<string> queue)
        {
            Thread[] threads = new Thread[nThreads];
            for (int i = 0; i < nThreads; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    string value = "ThreadId " + threadId;
                    queue.Put(value);
                });
                threads[i].Start();
            }
         

            for (int i = 0; i < nThreads; i++)
            {
                if (threads[i] != null)
                    threads[i].Join();
            }

            string result = queue.TestGetQueue();
            Console.WriteLine(result);
            Console.ReadKey();
        }

        public static void GetThreadedTest(ConcurrentQueue<string> queue)
        {
            Thread[] threads = new Thread[nThreads];
            for (int i = 0; i < nThreads; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    string value = queue.TryTake();
                    
                    for(int f  = 0; f< threadId; ++f)
                    {
                        threads[f].Join();
                    }
                    Console.WriteLine("Thread " + threadId + " get completed with result: " + value);
                });
                threads[i].Start();
            }

            for (int i = 0; i < nThreads; i++)
            {
                if (threads[i] != null)
                    threads[i].Join();
            }

            Thread releaseThread = new Thread(() =>
            {
                for (int i = 0; i < nThreads; i++)
                {
                    if (threads[i] == null)
                        continue;
                    while (threads[i].ThreadState == ThreadState.Running)
                        Thread.Sleep(1000);
                }
            });
            string result = queue.TestGetQueue();
            
            Console.WriteLine(result);
            Console.ReadKey();
        }

        public static void GetTest(ConcurrentQueue<string> queue)
        {
            for(int i = 0; i < nThreads; i++)
            {
                Console.WriteLine(queue.TryTake());
            }
            Console.ReadKey();
        }

        public static void Main()
        {
            
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            PutThreadedTest(queue);
            GetThreadedTest(queue);
            //GetTest(queue);
        }
    }
}
