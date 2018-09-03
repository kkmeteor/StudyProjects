using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = ConfigManager.ConfigManager.GetValue("abc");
            var b = ConfigManager.ConfigManager.GetValue("abc");
            Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaa");
            Console.Read();
        }
    }
}
