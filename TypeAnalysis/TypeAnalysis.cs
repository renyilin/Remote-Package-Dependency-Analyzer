///////////////////////////////////////////////////////////////////////
// Parser.cs - Parser detects code constructs defined by rules       //
// ver 1.5                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
//              Yilin Ren, yren20@syr.edu                            //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Parser  - a collection of IRules
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, Parser.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.5 : 14 Oct 2014
 * - added bug fix to tokenizer to avoid endless loop on
 *   multi-line strings
 * ver 1.4 : 30 Sep 2014
 * - modified test stub to display scope counts
 * ver 1.3 : 24 Sep 2011
 * - Added exception handling for exceptions thrown while parsing.
 *   This was done because Toker now throws if it encounters a
 *   string containing @".
 * - RulesAndActions were modified to fix bugs reported recently
 * ver 1.2 : 20 Sep 2011
 * - removed old stack, now replaced by ScopeStack
 * ver 1.1 : 11 Sep 2011
 * - added comments to parse function
 * ver 1.0 : 28 Aug 2011
 * - first release
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lexer;

namespace DepAnalysis
{
  /////////////////////////////////////////////////////////
  // rule-based parser used for code analysis

  public class Parser
  {
    private List<IRule> Rules;

    public Parser()
    {
      Rules = new List<IRule>();
    }
    public void add(IRule rule)
    {
      Rules.Add(rule);
    }
    public void parse(Lexer.ITokenCollection semi)
    {
      // Note: rule returns true to tell parser to stop
      //       processing the current semiExp
      
      Display.displaySemiString(semi.ToString());

      foreach (IRule rule in Rules)
      {
        if (rule.test(semi))
          break;
      }
    }
  }

  public class TestParser
  {
    //----< process commandline to get file references >-----------------

    public static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length == 0)
      {
        Console.Write("\n  Please enter file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
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

    //----< Test Stub >--------------------------------------------------

#if(TEST_TYPEANALYSIS)

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Parser");
      Console.Write("\n ======================\n");

      ShowCommandLine(args);

      List<string> files = TestParser.ProcessCommandline(args);
            //Repository repo = Repository.getInstance();
      Repository repo = new Repository();
      repo.semi = Factory.create();

      foreach (string file in files)
      {
        Console.Write("\n  Processing file {0}", System.IO.Path.GetFileName(file));

        //ITokenCollection semi = Factory.create();
        //semi.displayNewLines = false;
        if (!repo.semi.open(file as string))
        {
          Console.Write("\n  Can't open {0}\n\n", args[0]);
          return;
        }

        BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
        Parser parser = builder.build();
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
      }

        Console.Write("\n\n  Type and Dependency Analysis");
        Console.Write("\n ----------------------------");

        TypeTable table = repo.typeTable;
        Display.showTypeTable(table);

        Console.Write("\n");
        Console.Write("\n\n");
    }
#endif
  }
}
