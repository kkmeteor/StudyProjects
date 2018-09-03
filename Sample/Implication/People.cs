using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample
{
    public class People : IFly
    {
        public string Fly(string name)
        {
            return name + ",you can fly!";
        }


        public string Run(string name)
        {
            return name + ",you can run!";
        }
    }
}
