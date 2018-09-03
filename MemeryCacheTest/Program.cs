using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeryCacheTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dic = MemeryCache.GetConfigDictionary("TimeOut");

            Console.Read();
        }
    }
}
