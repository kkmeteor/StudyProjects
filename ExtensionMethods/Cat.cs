using IOCStudy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public class Cat : IRunDao
    {
        string IRunDao.Run()
        {
            return "11111111111111111111111";
        }
    }
}
