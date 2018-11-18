using System;
using System.Collections.Generic;
using N1;
using A2 = N4.C6;

namespace N2    // the same namespace with TC2
{

    public class C4
    {
        public class C5      // Internal Class
        {
            public void fun2()
            {
                N3.C2 c2 = new N2.N3.C2();   //Using fully qualified name
            }
        }
    }

    public class C1
    {
        public void fun0()
        {
            A2 c2 = new A2();   //Using Alias
        }
    }

    public struct Office
    {
        List<string> employee;
        List<string> leader;
        public Office(List<string> employee, List<string> leader)
        {
            this.employee = employee;
            this.leader = leader;
        }
    }

}
