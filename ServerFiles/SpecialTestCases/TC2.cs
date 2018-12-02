using System;
using N1;

namespace N2
{
    public class C3 
    {
        static void fun1()
        {
            // refer Internal Class
            // refer type in the same namespace with different packages
            C4.C5 a = new C4.C5();   
        }
    }

    namespace N3  //Nested namespaces
    {
        public class C2
        {


        }
    }
}
