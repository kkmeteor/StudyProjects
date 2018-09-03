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
                Console.WriteLine("hello,world");
            },TaskCreationOptions.LongRunning);
            Console.Read();
            var task1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
            }).ContinueWith((t) =>
            {
                Console.WriteLine("hello,matengfei");
            },TaskScheduler.FromCurrentSynchronizationContext());
            Console.Read();
        }
    }
}
