using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskFactoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                Console.WriteLine("hello,world");
                PrintThread();
            }, TaskCreationOptions.LongRunning);
            var task1 = Task.Factory.StartNew(() =>
            {

                Thread.Sleep(1000);
                Console.WriteLine("hello,matengfei1");
                PrintThread();
            }).ContinueWith((t) =>
            {
                Console.WriteLine("hello,matengfei2");
                PrintThread();
            });
            var task2 = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("hello,matengfei3");
                PrintThread();
            });
            Console.Read();
        }
        static void PrintThread()
        {
            Console.WriteLine("Main：线程Id:【{0}】是否后台线程:【{1}】是否使用线程池:【{2}】当前时间:【{3}】",
                Environment.CurrentManagedThreadId.ToString(),
                Thread.CurrentThread.IsBackground,
                Thread.CurrentThread.IsThreadPoolThread,
                DateTime.Now.ToString());
        }
    }
}
