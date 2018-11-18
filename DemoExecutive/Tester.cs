///////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Prototype Code Analyzer                //
// ver 1.0                                                           //
// Language:    C#, 2017, .Net Framework 4.7.1                       //
// Platform:    Dell Precision T8900, Win10                          //
// Application: Demonstration for CSE681, Project #3, Fall 2018      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   Executive:
 *   - uses Parser, RulesAndActions, Semi, and Toker to perform basic
 *     code metric analyzes
 */
/* Required Files:
 *   Executive.cs
 *   Parser.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 09 Oct 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsGraph;

namespace DepAnalysis
{
  using Lexer;

  class Executive
  {

    public static List<string> ProcessCommandline(string[] args)
    {
        List<string> files = new List<string>();
        if (args.Length < 1)
        {
            Console.Write("\n  Please enter path to analyze\n\n");
            return files;
        }
        string path = args[0];
        if (!Directory.Exists(path))
        {
            Console.Write("\n  invalid path \"{0}\"", Path.GetFullPath(path));
            return files;
        }
        path = Path.GetFullPath(path);
        files = Executive.findFiles(path, "*.cs");

        return files;
    }

      public static List<string> findFiles(string searchpath, string pattern)
    {
        List<string> fileList = Directory.GetFiles(searchpath, pattern).ToList();
        foreach (string dir in Directory.GetDirectories(searchpath))
        {
            if (dir == "." || dir == "..")
                continue;
            fileList.AddRange(findFiles(dir, pattern));
        }
        return fileList;
    }

      public static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n");
    }

      public static void BuildTypeTable(string[] args, List<string> files, Repository repo)
    {
        BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
        Parser parser = builder.build();

        foreach (string file in files)
        {
            Console.Write("\n  Processing file {0}", System.IO.Path.GetFileName(file));

            if (!repo.semi.open(file as string))
            {
                Console.Write("\n  Can't open {0}\n\n", args[0]);
                return;
            }

            try
            {
                while (repo.semi.get().Count > 0)
                    parser.parse(repo.semi);
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }

            repo.clearScopeStack();
            repo.semi.close();

            CsNode<FileNode, string> fileNode = new CsNode<FileNode, string>(repo.semi.filePath);
            fileNode.nodeValue = new FileNode(repo.semi.filePath);
            repo.nodeTable.Add(repo.semi.filePath, fileNode);
            repo.depGraph.addNode(repo.nodeTable[repo.semi.filePath]);
        }

    }

     public static void depAnalysis(string[] args, List<string> files, Repository repo)
    {
        BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
        Parser parser = builder.build();

        BuildDepAnalyzer depBuilder = new BuildDepAnalyzer(repo);
        Parser depParser = depBuilder.build();

        foreach (string file in files)
        {
            if (!repo.semi.open(file as string))
            {
                Console.Write("\n  Can't open {0}\n\n", args[0]);
                return;
            }

            try
            {
                while (repo.semi.get().Count > 0)
                {
                    parser.parse(repo.semi);
                    depParser.parse(repo.semi);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }

            repo.clearScopeStack();
            repo.semi.close();
        }

    }


     static void Main(string[] args)
    {
        Console.Write("\n  Demonstrating Type-Based Package Dependency Analysis");
        Console.Write("\n ======================================================\n");

        ReqTest3 reqTest3 = new ReqTest3();
        ReqTest4 reqTest4 = new ReqTest4();
        ReqTest5 reqTest5 = new ReqTest5();
        ReqTest6 reqTest6 = new ReqTest6();
        ReqTest7 reqTest7 = new ReqTest7();
        ReqTest8 reqTest8 = new ReqTest8();

        TestHarness.Tester tester = new TestHarness.Tester();
        tester.add(reqTest3);
        tester.add(reqTest4);
        tester.add(reqTest5);
        tester.add(reqTest6);
        tester.add(reqTest7);
        tester.add(reqTest8);

        tester.execute();

        Console.Read();
    }

 }
}
