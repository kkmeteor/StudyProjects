using Client.PeopleService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            PeopleService.FlyClient client = new FlyClient("NetTcpBinding_IFly");
            //FlyClient client = new FlyClient();
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Console.WriteLine(client.Fly("MaTengfei"));
            watch.Stop();
            Console.WriteLine("NetTcpWCF调用用时：" + watch.ElapsedMilliseconds);
            PeopleService.FlyClient client1 = new FlyClient("WSHttpBinding_IFly");
            //FlyClient client = new FlyClient();
            System.Diagnostics.Stopwatch watch1 = new System.Diagnostics.Stopwatch();
            watch1.Start();
            Console.WriteLine(client.Fly("MaTengfei"));
            watch.Stop();
            Console.WriteLine("WSHttpWCF调用用时：" + watch1.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
