using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;


namespace MyClient
{
    class MyClient
    {

        [ServiceContract(Namespace = "MyService")]
        interface IService
        {
            [OperationContract]
            string Hello(string name);

            void abc();
        }


        static void Main(string[] args)
        {
            EndpointAddress address = new EndpointAddress("http://localhost:8080/");
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<IService>  factory = new ChannelFactory<IService>(binding, address);
            IService channel = factory.CreateChannel();

            channel.Hello("FYL");
            //channel.abc();

            //Console.WriteLine("Client: "+str);
            Console.Read();
        }
    }
}
