using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class People : ISpeak
    {
        string ISpeak.Speak()
        {
            return "111111111111111";
        }
    }
}
