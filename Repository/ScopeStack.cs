/////////////////////////////////////////////////////////////////////////
// ScopeStack.cs  -  Generic stack to help with static analysis        //
//                   Holds application specific Element type           //
// ver 1.0                                                             //
// Language:    C#, Visual Studio 10.0, .Net Framework 4.0             //
// Platform:    Dell Precision T7400 , Win 7, SP 1                     //
// Application: Pr#2 Help, CSE681, Fall 2011                           //
// Author:      Jim Fawcett, CST 2-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * ScopeStack provides, via the class ScopeStack, the facilities to 
 * track position in code scope by pushing and popping type, name, and
 * place.  The type might be a namespace, class, struct, function, or
 * control.  The name is the namespace of the instance of that type,
 * and place is the line number in a package being analyzed.
 * 
 * Public Interface
 * ================
 * ScopeStack sstk = new ScopeStack();  // constructs ScopeStack object
 * E elem = new E(); elem.type = "xyz", elem.name = "tuv", elem.place = 53;
 * sstk.push(e);                        // sstk now holds that element
 * E e = sstk.pop();                    // pops element off stack into e
 * int size = sstk.count;               // number of elements on sstk
 * E e = sstk[3];                       // peering inside stack
 * sstk.display();                      // puts vertical list of elements on console
 *                                      //   using E.ToString()
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   ScopeStack.cs
 * 
 * Compiler Command:
 *   csc /target:exe /define:TEST_SCOPESTACK ScopeStack.cs
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 05 Sep 11
 *   - first release
 * 
 */
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepAnalysis
{
  public class ScopeStack<E>
  {
    List<E> stack_ = new List<E>();
    E lastPopped_;

    //----< push element onto stack >------------------------------------

    public void push(E elem)
    {
      stack_.Add(elem);
    }
    //----< pop element off of stack >-----------------------------------

    public E pop()
    {
      int len = stack_.Count;
      if (len == 0)
        throw new Exception("empty scope stack");
      E elem = stack_[len - 1];
      stack_.RemoveAt(len - 1);
      lastPopped_ = elem;
      return elem;
    }
    //----< remove all elements from stack >-----------------------------

    public void clear()
    {
      stack_.Clear();
    }
    //----< index into stack contents >----------------------------------

    public E this[int i]
    {
      get
      {
        if (i < 0 || stack_.Count <= i)
          throw new Exception("scope stack index out of range");
        return stack_[i];
      }
      set
      {
        if (i < 0 || stack_.Count <= i)
          throw new Exception("scope stack index out of range");
        stack_[i] = value;
      }
    } 

    //----< number of elements on stack property >-----------------------

    public int count
    {
      get { return stack_.Count; }
    }
    //----< get lastPopped >---------------------------------------------

    public E lastPopped()
    {
      return lastPopped_;
    }
    //----< display using element ToString() method() >------------------

    public void display()
    {
      for (int i = 0; i < count; ++i)
      {
        Console.Write("\n  {0}",stack_[i].ToString());
      }
    }
  }

  class Test
  {
    public struct Elem 
    {
      public string type;
      public string name;
      public int place;
      public void make(string tp, string nm, int pl)
      {
        type = tp;
        name = nm;
        place = pl;
      }
      public override string ToString()
      {
        StringBuilder temp = new StringBuilder();
        temp.Append("{");
        temp.Append(String.Format("{0,-10}",type)).Append(" : ");
        temp.Append(String.Format("{0,-10}",name)).Append(" : ");
        temp.Append(String.Format("{0,-5}",place.ToString()));
        temp.Append("}");
        return temp.ToString();
      }
    };

#if(TEST_SCOPESTACK)
    static void Main()
    {
      Console.Write("\n  Test ScopeStack");
      Console.Write("\n =================\n");

      ScopeStack<Elem> mystack = new ScopeStack<Elem>();
      Test.Elem e;
      e.type = "namespace";
      e.name = "foobar";
      e.place = 14;
      mystack.push(e);
      e.make("class", "feebar", 21);
      mystack.push(e);
      e.make("function", "doTest", 44);
      mystack.push(e);
      e.make("control", "for", 56);
      mystack.push(e);

      mystack.display();
      Console.WriteLine();

      Elem test = mystack.lastPopped();
      Console.Write("\n  last popped:\n  {0}\n", test);

      e = mystack.pop();
      Console.Write("\n  popped:\n  {0}", e);
      e = mystack.pop();
      Console.Write("\n  popped:\n  {0}\n", e);
      Console.Write("\n  last popped:\n  {0}\n", mystack.lastPopped());

      mystack.display();

      Console.Write("\n\n");
    }
#endif
  }
}
