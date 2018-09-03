using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Task<Int32> t = new Task<Int32>(n => Sum((Int32)n), 20);

            Task<string> s = new Task<string>(b => Get((string)b), "matfb");
            t.Start();
            s.Start();
            Task cwt = t.ContinueWith(task => Console.WriteLine("The Intresult is {0}", t.Result));
            Task cwt1 = s.ContinueWith(task => Console.WriteLine("The Stringresult is {0}", s.Result));
            //ts.Start();

            //Task cct = tt.ContinueWith(task => Console.WriteLine("The TaskId is {0}", tt.Id));

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, "aaaaaabbbbb");
            ms.Position = 0;
            string s1 = formatter.Deserialize(ms) as string;
            Console.ReadKey();
        }

        private static string Get(string p)
        {
            return p.Substring(p.Length - 1, 1);
        }

        private static string GetLast(Task s, string p)
        {
            throw new NotImplementedException();
        }

        private static int GetInt(Task<string> i, int p)
        {
            Console.WriteLine("s.Result" + i.Result);
            return p + 5;
        }

        private static string GetLast(Task<int> s, string p)
        {
            Console.WriteLine("s.Result"+s.Result);
            return p.Substring(p.Length - 1, 1);
        }

        private static void Sum(object state)
        {
            throw new NotImplementedException();
        }

        private static string GetLast(string p)
        {
            return p.Substring(p.Length - 1, 1);
        }

        private static int Sum(int n)
        {
            Int32 sum = 0;
            for (; n > 0; --n)
            {
                Thread.Sleep(100);
                checked { sum += n; } //结果太大，抛出异常
            }
            return sum;
        }
    }
}
