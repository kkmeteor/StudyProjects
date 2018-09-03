using MSMQWCF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.ServiceModel;
using System.Text;

namespace MSMQHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var queueName = ConfigurationManager.AppSettings["queueName"];
            if (!MessageQueue.Exists(queueName))
                MessageQueue.Create(queueName);
            ServiceHost host = new ServiceHost(typeof(Order));
            host.Open();
            Console.WriteLine("MSMQWCF服务已经启动！");
            Console.ReadLine();
        }
    }
}
