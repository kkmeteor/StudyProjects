using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using TestProject;

namespace TestProjectClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //使用HTTP通道得到远程对象
            HttpChannel chan2 = new HttpChannel();
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(chan2,false);
            HelloServer obj1 = (HelloServer)Activator.GetObject(
                typeof(TestProject.HelloServer),
                "http://localhost:8086/SayHello");//创建类的实例
            if (obj1 == null)
            {
                System.Console.WriteLine(
                    "Could not locate HTTP server");
            }
            Console.WriteLine(
                "Client HTTP HelloUserMethod {0}",
                obj1.HelloUserMethod(new User("张生", true))); //将类作为参数（将User作为参数必须是serializable)
            TcpChannel tcpChannel = new TcpChannel();

            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(tcpChannel, false);
            HelloServer obj2 = (HelloServer)Activator.GetObject(
               typeof(TestProject.HelloServer),
               "tcp://localhost:8086/SayHello");//创建类的实例
        }
    }
}
