using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStudy1
{
    public class Boiler
    {
        private int temp;
        private int presure;

        public Boiler(int v1, int v2)
        {
            temp = v1; presure = v2;
        }

        public int GetTemp()
        {
            return this.temp;
        }
        public int GetPresure()
        {
            return presure;
        }
    }
}
