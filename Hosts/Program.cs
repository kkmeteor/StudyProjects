using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Hosts
{
    class Program
    {
        static void Main(string[] args)
        {
            using(ServiceHost host = new ServiceHost(typeof(Sample.People)))
            {
                host.Open();
                Console.WriteLine("WCF--FlyPeole服务已经启动");
                Console.ReadLine();
            }

        }
    }
}
