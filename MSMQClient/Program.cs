using MSMQWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MSMQClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ChannelFactory<IOrder>(new NetMsmqBinding(NetMsmqSecurityMode.None),
                new EndpointAddress(@"net.msmq://localhost/private/order"));
            var orderClient = factory.CreateChannel();
            for (int i = 0; i < 100; i++)
            {
                orderClient.Add("MaTengfei下达了一条订单，订单号码为" + i.ToString());
            }
            
        }
    }
}
