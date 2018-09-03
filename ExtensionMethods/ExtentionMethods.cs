using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class ExtentionMethods
    {
        public static int WordCount(this string str)
        {
            return str.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static bool Run(this Bird bird)
        {
            return true;
        }

        public static bool Run(this IFly flyer)
        {
            
            return true;
        }
    }
}
