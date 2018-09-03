using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LambdaStudy
{
    class Program
    {
        delegate int del(int i); 
        static void Main(string[] args)
        {
            Expression<del> myET = x => x * x; 
            del myDelegate = x => x * x;
            int j = myDelegate(5);
            Console.WriteLine(j);
            Console.ReadKey();
        }
    }
}
