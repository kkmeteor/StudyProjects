using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Sample
{
    [ServiceContract]
    internal interface IRun
    {
        [OperationContract]
        string Run(string name); 
    }
}
