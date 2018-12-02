using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBasicWCF
{
    class MyService : IService
    {
        public void abc()
        {
            Console.WriteLine("abc");
        }

        public string Hello(string name)
        {
            Console.WriteLine("Hello " + name);
            return "Hello " + name;
        }

    }
}
