using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Debugger.Launch();
            F6();
            Console.ReadKey();
        }
        public class Class1
        {
            private static Class1 _instance;
            readonly int intValue = 1;
            readonly string strValue = "1";
            private Class1(int intValue,string stringValue)
            {
                this.intValue = intValue;
                this.strValue = stringValue;
            }
            public static Class1 GetInstance()
            {
                if (_instance == null)
                    _instance = new Class1(1,"1");
                return _instance;
            }
        }
        private static  void F6()
        {
            List<int> intList = new List<int>();
            Console.WriteLine(intList.Any());
            intList.Add(1);
            Console.WriteLine(intList.Any());
            List<Class1> classList = new List<Class1>();
            Console.WriteLine(classList.Any());
            classList.Add(Class1.GetInstance());
            Console.WriteLine(classList.Any());
        }
        private static void F5()
        {
            var list = FindNum();
            foreach(var i in list)
            {
                Console.WriteLine(i);
            }
        }

        /// <summary>
        /// 找100以内质数
        /// </summary>
        /// <returns></returns>
        private static List<int> FindNum()
        {
            List<int> list = new List<int>();
            int i, j;
            for (i = 2; i < 100; i++)
            {
                for (j = 2; j <= i / 2; j++)
                {
                    if (i % j == 0)
                        break;
                }
                if (j > i / 2)
                {
                    list.Add(i);
                    Console.WriteLine("质数: " + i);
                }
            }
            return list;
        }
        /// <summary>
        /// 九九乘法表算法实现
        /// </summary>
        static void F4()
        {
            string t = string.Empty;
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    t = string.Format("{0}*{1}={2} ", j, i, (j * i));
                    Console.Write(t);
                    //if (j * i < 82)    
                    //    Console.Write(" ");    
                    if (i == j)
                        Console.Write("\n");
                }
            }
        }    
        /// <summary>
        /// 递归算法
        /// </summary>
        private static void F3()
        {
            int n = Func(30);
            Console.WriteLine("第30个数字是：" + n.ToString());
        }

        private static int Func(int p)
        {
            if (p <= 2)
                return 1;
            else
                return Func(p - 1) + Func(p - 2);
        }

        private static void F2()
        {
            for (int a = 0; a < 10; a++)
            {
                for (int b = 0; b < 10; b++)
                {
                    for (int c = 0; c < 10; c++)
                    {
                        if ((120560 + a + b * 1000) * 123 == 15404987 + c * 10000)
                        {
                            Console.WriteLine(a);
                            Console.WriteLine(b);
                            Console.WriteLine(c);
                        }
                    }
                }
            }
        }

        //有1、2、3、4个数字，能组成多少个互不相同且无重复数字的三位数？都是多少？    
        //分解题目    
        //条件：四个数字1、2、3、4  ；三位数：百位、十位、个位    
        //要求：互不相同；无重复数字：每个数字在三位中只出现一次    
        //结果：多少个？ 都是多少？   
        private static void F1()
        {
             

            int count = 0; //统计个数    
            for (int bw = 1; bw <= 4; bw++)
            {
                for (int sw = 1; sw <= 4; sw++)
                {
                    if (sw != bw)  //很显然，只有百位和十位不同的情况下才能谈个位。    
                    {
                        for (int gw = 1; gw <= 4; gw++)
                        {
                            if (gw != sw && gw != bw)   //百位用过的，十位就不能用；百位和十位都用过的，个位就不能用    
                            {
                                count++;
                                Console.WriteLine("{0}{1}{2}", bw, sw, gw);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("一共有{0}个", count);
        }
    }
}
