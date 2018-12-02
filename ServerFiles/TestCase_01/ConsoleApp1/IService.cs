using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace MyBasicWCF
{
    [ServiceContract(Namespace = "MyService")]
    interface IService
    {
        [OperationContract]
        string Hello(string name);

        void abc();
    }
}
