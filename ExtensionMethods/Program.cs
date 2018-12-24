using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    class Program
    {
        static void Main(string[] args)
        {
            var guid = Guid.NewGuid();
            string s = "Hello world!,my dear";
            BigBird bb = new BigBird();
            int a = s.ToInt();
            Console.WriteLine(bb.Run().ToString());
            Console.WriteLine(s.WordCount());
            Console.WriteLine(new Bird().Run());
            Console.Read();
        }
    }
}
