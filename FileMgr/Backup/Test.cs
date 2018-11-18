///////////////////////////////////////////////////////////////////////
///  Test.cs    -  Demonstrates use of System.IO classes            ///
///  ver 1.0       Uses Navigate Delegates to get event callbacks   ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Dell Dimension 8100, Windows Pro 2000, SP2       ///
///  Application:  CSE681 Example                                   ///
///  Author:       Jim Fawcett, CST 2-187, Syracuse Univ.           ///
///                (315) 443-3948, jfawcett@twcny.rr.com            ///
///////////////////////////////////////////////////////////////////////
/*
 *   Operations:
 *  =============
 *   This is a test driver for Navigate.  It provides event handlers
 *   to react to Navigates file and directory events.
 * 
 */

using System;
using System.IO;

namespace Navig
{
  class Test
  {
    Test()
    {
    }
    void OnDir(string dir)
    {
      Console.Write("\n\n  {0}",dir);
    }
    void OnFile(string file)
    {
      string name = Path.GetFileName(file);
      FileInfo fi = new FileInfo(file);
      DateTime dt = File.GetLastWriteTime(file);
      Console.Write("\n   {0,-35} {1,8} bytes  {2}",name,fi.Length,dt);
    }
    void Register(Navigate nav)
    {
      nav.newDir += new Navigate.newDirHandler(OnDir);
      nav.newFile += new Navigate.newFileHandler(OnFile);
    }
    [STAThread]
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrate Directory Navigation with Delegates ");
      Console.Write("\n =================================================");

      string path;
      if(args.Length > 0)
        path = args[0];
      else
        path = Directory.GetCurrentDirectory();
      Navigate nav = new Navigate();

      Test test = new Test();
      test.Register(nav);
      nav.go(path, "*.*");

      Console.Write("\n\n");
    }
  }
}
