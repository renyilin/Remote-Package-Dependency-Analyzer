using System;
using Lexer;
using nsPkgA;
using nsPkgC;

namespace nsPkgB
{
	public class ClassA
	{
		
	}
	
	public class ClassC
	{
		void fun1()
        {
            Office office = new Office();
            office.ToString();
        }
	}
	
	public class Dog //: Animal
	{
		
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
}
