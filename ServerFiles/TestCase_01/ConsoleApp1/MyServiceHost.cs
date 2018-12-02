using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace MyBasicWCF
{
    class MyServiceHost
    {
        static void Main(string[] args)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri("http://localhost:8080/");
            ServiceHost commHost = new ServiceHost(typeof(MyService), baseAddress);
            commHost.AddServiceEndpoint(typeof(IService), binding, baseAddress);
            commHost.Open();

            Console.WriteLine("Service is ready.");
            Console.Read();
        }
    }
}
