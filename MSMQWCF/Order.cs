using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MSMQWCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Order : IOrder
    {
        public void Add(string order)
        {
            try
            {
                if (!File.Exists("D://1.txt"))
                    File.Create("D://1.txt");
                File.AppendAllText("D://1.txt", order);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
