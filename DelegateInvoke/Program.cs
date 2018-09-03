using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DelegateInvoke
{
    class Program
    {
        public delegate ABC MyAction(int i);
        static void Main(string[] args)
        {
            var d = new MyAction(xxx);
            var asyncResult = d.BeginInvoke(2000, ad, d);
            //while (!asyncResult.AsyncWaitHandle.WaitOne(100, false))
            //{
            //    Console.Write("*");
            //}
            //var result = d.EndInvoke(asyncResult);
            //d.BeginInvoke(h => { Console.WriteLine("返回值是{0}", d.EndInvoke(h).result); }, null);
            //Console.WriteLine("返回值是{0}", result.result);
            Console.ReadKey();
        }

        private static void ad(IAsyncResult ar)
        {
            if (ar == null) return;
            ABC result = (ar.AsyncState as MyAction).EndInvoke(ar);
            string result1 = result.ToString();
            Console.WriteLine("委托内的方法执行完毕了");
        }

        private static ABC xxx(int i)
        {
            Thread.Sleep(1000);
            return new ABC();
        }
    }
}
