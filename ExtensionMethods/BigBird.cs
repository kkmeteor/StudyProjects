﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public class BigBird : IFly
    {
        bool IFly.Fly()
        {
            return false;
        }
    }
}
