using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample
{
    public class RunPeople:IRun
    {
        public string Run(string name)
        {
            return name + ",you can Run!";
        }
    }
}
