// TestHarness.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITest
{
  bool doTest();
  bool result { get; set; }
  string name { get; set; }
}

namespace TestHarness
{
  public class Tester
  {
    public List<ITest> tests { get; set; } = new List<ITest>();
    public bool result { get; set; } = true;

    public void add(ITest test)
    {
      tests.Add(test);
    }
    public void clear()
    {
      tests.Clear();
    }
    public void execute()
    {
      foreach(ITest test in tests)
      {
        Executor.check(Executor.execute(test), test.name);
        if (!test.result)
          result = false;
      }
    }
    public void check()
    {
      if (result)
        Console.Write("\n  all tests passed\n");
      else
        Console.Write("\n  one or more tests failed\n");
    }
  }
  public class Executor
  {
    public static bool execute(ITest test)
    {
      bool result = false;
      try
      {
        result = test.doTest();
        test.result = result;
      }
      catch
      {
        test.result = false;
      }
      return result;
    }
    public static void check(bool result, string name)
    {
      if (result)
        Console.Write("\n  passed -- \"{0}\"\n", name);
      else
        Console.Write("\n  failed -- \"{0}\"\n", name);
    }
  }
  class Test1 : ITest
  {
    public bool result { get; set; } = false;
    public string name { get; set; } = "Test1 - always passes";
    public bool doTest()
    {
      return true;
    }
  }
  class Test2 : ITest
  {
    public bool result { get; set; } = false;
    public string name { get; set; } = "Test2 - always fails";
    public bool doTest()
    {
      return false;
    }
  }
  class Test3 : ITest
  {
    public bool result { get; set; } = false;
    public string name { get; set; } = "Test3 - always throws";
    public bool doTest()
    {
      Exception ex = new Exception("test throw");
      throw ex;
    }
  }
  class Test_TestHarness
  {
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrate TestHarness");
      Console.Write("\n =========================\n");

      Console.Write("\n  Testing Executor");
      Console.Write("\n ------------------");

      ITest t1 = new Test1();
      Executor.check(Executor.execute(t1), t1.name);
      ITest t2 = new Test2();
      Executor.check(Executor.execute(t2), t2.name);
      ITest t3 = new Test3();
      Executor.check(Executor.execute(t3), t3.name);

      Console.Write("\n  Testing Tester");
      Console.Write("\n ------------------");

      Tester tester = new Tester();
      tester.add(t1);
      tester.add(t2);
      tester.add(t3);
      tester.execute();
      tester.check();
      
      Console.Write("\n\n");
    }
  }
}
