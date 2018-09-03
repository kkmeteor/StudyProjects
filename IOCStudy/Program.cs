using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCStudy
{
    class Program
    {
        static void Main(string[] args)
        {
            var userService = UserService.Instance();
            
            Console.WriteLine(userService.Add(10));
            Console.WriteLine(userService.Fly().ToString());
            Console.ReadKey();
        }
    }
}
