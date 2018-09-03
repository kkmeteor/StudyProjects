using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class HelloServer
        : MarshalByRefObject
    {
        public HelloServer()
        { Console.WriteLine("HelloServer activated"); }


        public String HelloUserMethod(User user)
        {
            string title;
            if (user.Male)
                title = "先生";
            else
                title = "女士";

            Console.WriteLine("Server Hello.HelloMethod : 你好，{0}{1}", user.Name, title);

            return "你好，" + user.Name + title;
        }
    }
}
