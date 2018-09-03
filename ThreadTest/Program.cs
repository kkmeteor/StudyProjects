using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread t1 = new Thread(new ThreadStart(TestMethod));
            Thread t2 = new Thread(new ParameterizedThreadStart(TestMethod));
            t1.IsBackground = true;
            t2.IsBackground = true;
            t1.Start();
            t2.Start("hello");
            Console.ReadKey();
        }

        private static void TestMethod(object obj)
        {
            Thread.Sleep(1000);
            string str = obj.ToString();
            Console.WriteLine("带参数的线程函数");
            Console.WriteLine("obj=" + obj);
        }

        private static void TestMethod()
        {
            Console.WriteLine("不带参数的线程函数");
        }
    }
}
