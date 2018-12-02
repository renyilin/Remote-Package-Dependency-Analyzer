using System.IO;
using N1;
using A1 = N2.N3;  //Alias and Nested namespaces

namespace N1
{
    public class C1 : N4.I1, N5.I2   //Implementing multiple interface 
    {
        public void fun0()
        {
            A1.C2 c2 = new A1.C2();  //Alias
        }
    }

    enum WeekDays
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }

    public delegate int PerformCalculation(int x, int y);
}
