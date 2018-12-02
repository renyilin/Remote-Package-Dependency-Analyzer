using System;
using Lexer;

namespace nsPkgC
{
	public class Animal : nsPkgA.ClassA
	{
	    nsPkgB.ClassA classA = new nsPkgB.ClassA();
	}
	
	public interface ICanFly
	{
		
	}

    public delegate int PerformCalculation(int x, int y);

    namespace nsCA
    {
        public class Forest
        {
        
        }
    }
}
