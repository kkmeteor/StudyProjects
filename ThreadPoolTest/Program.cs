using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreadPoolTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(TestMethod, "hello");
            ThreadPool.QueueUserWorkItem(TestMethod1);
            Console.ReadKey();
        }

        private static void TestMethod1(object state)
        {
            string datastr = state as string;
            Console.WriteLine("TestMethod1"+datastr);
        }

        private static void TestMethod(object data)
        {
            string datastr = data as string;
            Console.WriteLine("TestMethod" + datastr);
        }
    }
}
