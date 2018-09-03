using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CloneDemo
{
    class Program
    {
        
        static void Main(string[] args)
        {
            DemoClass dc = new DemoClass();
            dc.i = 0;
            dc.ints = new int[3] { 7, 9, 10 };
            DemoClass b = dc.Clone1();
            DemoClass c = dc.Clone2();
            dc.i = 999;
            dc.ints[0] = 888;
            Console.WriteLine("b.i = " + b.i.ToString());
            Console.WriteLine("c.i = " + c.i.ToString());
            Console.WriteLine("b.ints = " );
            foreach(int i in b.ints)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("c.ints = ");
            foreach (int i in c.ints)
            {
                Console.WriteLine(i);
            }
            Console.ReadKey();
        }
    }
}
