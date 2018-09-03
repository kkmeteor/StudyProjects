using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class Server
    {
        static void Main(string[] args)
        {
            TcpChannel chan1 = new TcpChannel(8085);
            HttpChannel chan2 = new HttpChannel(8086);

            ChannelServices.RegisterChannel(chan1,false);
            ChannelServices.RegisterChannel(chan2, false);


            RemotingConfiguration.RegisterWellKnownServiceType(typeof(HelloServer), "SayHello", WellKnownObjectMode.Singleton);   //创建类的实例

            System.Console.WriteLine("Press Enter key to exit");
            System.Console.ReadLine();
        }
    }
}
