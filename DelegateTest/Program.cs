using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelegateTest
{
    public delegate string MyDelegate(object data);
    class Program
    {
        static void Main(string[] args)
        {
            MyDelegate mydelegate = new MyDelegate(TestMethod);
            IAsyncResult result = mydelegate.BeginInvoke("abcdefg", TestCallBack, "hijklmn");
            string resultstr = mydelegate.EndInvoke(result);
            Console.WriteLine(resultstr);
            Console.ReadKey();
        }

        private static void TestCallBack(IAsyncResult ar)
        {
            Console.WriteLine(ar.AsyncState);
        }

        private static string TestMethod(object data)
        {
            string datastr = data as string;
            return datastr;
        }

    }
}
