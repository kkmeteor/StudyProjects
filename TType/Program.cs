using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TType
{
    static class Program
    {
        static void Main(string[] args)
        {
            AAA a = new AAA();
            Type type = a.GetType();
            var aaa = Create<AAA>("aaa");
            aaa.AAAAAA();
            var bbb = Create<BBB>("bbb");
            bbb.BBBBBB();
            Console.ReadKey();

        }
        static T Create<T>(string connString) where T:Do
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] { connString });
        }
    }
    public class AAA
    {
        private string str;
        public AAA()
        {

        }
        public AAA(string input)
        {
            str = input;
        }
        public string AAAAAA()
        {
            return "AAA" + str;
        }
    }
    public class BBB:Do
    {
        private string str;
        public BBB(string input)
        {
            str = input;
        }
        public string BBBBBB()
        {
            return "BBB" + str;
        }

        override void Do.Do()
        {
            throw new NotImplementedException();
        }
    }

}
