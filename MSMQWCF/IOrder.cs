using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MSMQWCF
{
    [ServiceContract]
    public interface IOrder
    {
        [OperationContract(IsOneWay=true)]
        void Add(string order);
    }
}
