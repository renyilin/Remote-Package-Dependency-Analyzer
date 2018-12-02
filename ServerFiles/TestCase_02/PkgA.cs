using System;
using alias1 = nsPkgC.nsCA;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexer;
using nsPkgB;
using nsPkgC;

namespace nsPkgA
{
	public class ClassA
	{
        alias1.Forest forest = new alias1.Forest();
        
        public class InternalClassA
        {
        
        }
        
	}
	
    public class ClassB : ClassA
	{
		ClassC classC = new ClassC();
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
