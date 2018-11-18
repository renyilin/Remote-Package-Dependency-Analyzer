///////////////////////////////////////////////////////////////////////////
// Display.cs  -  Manage Display properties                              //
// ver 1.1                                                               //
// Language:    C#, Visual Studio 2013, .Net Framework 4.5               //
// Platform:    Dell XPS 2720 , Win 8.1 Pro                              //
// Application: Pr#2 Help, CSE681, Fall 2014                             //
// Author:      Jim Fawcett, CST 2-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
//              Yilin Ren                                                //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * Display manages static public properties used to control what is displayed and
 * provides static helper functions to send information to MainWindow and Console.
 * 
 * Public Interface
 * ================
 * Display.showConsole = false;  // disables most writing to console
 * Display.showFooter = true;    // enables status information display in footer
 * ...
 * Display.displayRules(act, ruleStr)  // sends ruleStr to console and/or footer
 * ...
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   FileMgr.cs
 *   
 * Compiler Command:
 *   devenv CSharp_Analyzer /rebuild debug
 * 
 * Maintenance History
 * ===================
 * ver 1.2 : 31 Oct 2018
 * ver 1.1 : 09 Oct 2018
 * - removed non-essential items from display
 * ver 1.0 : 19 Oct 2014
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CsGraph;
using System.IO;

namespace DepAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // StringExt static class
    // - extension method to truncate strings

    using ScopeList = List<string>;

    public static class StringExt
  {
    public static string Truncate(this string value, int maxLength)
    {
      if (string.IsNullOrEmpty(value)) return value;
      return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
  }

  static public class Display
  {
    static Display()
    {
      showFiles = true;
      showDirectories = true;
      showActions = false;
      showRules = false;
      useFooter = false;
      useConsole = false;
      goSlow = false;
      width = 33;
      showSemi = false;
      useConsole = false;
    }
    static public bool showFiles { get; set; }
    static public bool showDirectories { get; set; }
    static public bool showActions { get; set; }
    static public bool showRules { get; set; }
    static public bool showSemi { get; set; }
    static public bool useFooter { get; set; }
    static public bool useConsole { get; set; }
    static public bool goSlow { get; set; }
    static public int width { get; set; }

    //----< display results of Code Analysis >-----------------------

    static public void showMetricsTable(List<TypeElement> table)
    {
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "category", "name", "bLine", "eLine", "size", "cmplx"
      );
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "--------", "----", "-----", "-----", "----", "-----"
      );
      foreach (TypeElement e in table)
      {
        /////////////////////////////////////////////////////////
        // Uncomment to leave a space before each defined type
        // if (e.type == "class" || e.type == "struct")
        //   Console.Write("\n");

        Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          e.type, e.name, e.beginLine, e.endLine,
          e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1
        );
      }
    }
        //----< display results of Scope List >-----------------------
        static public string showScope(ScopeList scopeList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(scopeList[0]);
            for (int i = 1; i < scopeList.Count; i++ )
            {
                stringBuilder.Append(" : " + scopeList[i]);
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        //----< display results of the fullname of the alias >-----------------------
        static public string showAlias(List<string> scopeList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(scopeList[0]);
            for (int i = 1; i < scopeList.Count; i++)
            {
                stringBuilder.Append("." + scopeList[i]);
            }
            return stringBuilder.ToString();
        }

        //----< display TypeTable >-----------------------

        static public void showTypeTable(TypeTable table)
        {
            Console.Write("\n\n ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.Write("\n\t\t\t\t    Type Table");
            Console.Write("\n ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            Console.Write(
                    "\n{0, 10}{1, 25}{2, 25}{3, 20}",
                "Category", "Name", "Namespaces", "Filename"
            );
            Console.Write(
                    "\n{0,10}{1,25}{2, 25}{3, 20}",
                "--------", "----", "----------", "--------"
            );
            foreach (List<TypeElement> e in table.getTypeElements())
            {
                    /////////////////////////////////////////////////////////
                    // Uncomment to leave a space before each defined type
                    // if (e.type == "class" || e.type == "struct")
                    //   Console.Write("\n");
                    foreach (TypeElement item in e)
                    {
                        Console.Write(
                        "\n{0,10}{1,25}{2, 25}{3, 20}",
                        item.type, item.name, Display.showScope(item.scopeList), item.fileName);
                    }
            }
        }

        //----< display AliasTable >-----------------------
        static public void showAliasTable(Dictionary<string, Dictionary<string, List<string>>> aliasTable)
        {
            Console.Write("\n\n ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.Write("\n\t\t\t Alias Table");
            Console.Write("\n ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            Console.Write(
                        "\n {0, 10}{1, 25}{2, 20}",
                        "Alias Name", "Full Name", "File"
            );
            Console.Write(
                        "\n {0, 10}{1, 25}{2, 20}",
                        "----------",  "----------", "-----"
            );

            foreach (var f in aliasTable)
            {
                foreach (var dic in f.Value)
                {
                    Console.Write(
                    "\n {0, 10}{1, 25}{2, 20}",
                        dic.Key, Display.showAlias(dic.Value), Path.GetFileName(f.Key));
                }
            }
        }

        //----< display results of dependency among packages >-----------------------

        static public void showDependency(CsGraph<FileNode, string> DepGraph)
        {

            foreach(CsNode<FileNode, string> node in DepGraph.adjList)
            {
                if(node.children.Count != 0)
                {
                    //StringBuilder stringBuilder = new StringBuilder();
                    //stringBuilder.Append(node.name).Append(" depends on following: ");
                    Console.Write("\n- {0} depends on:", node.nodeValue.fileName);
                    foreach (var edge in node.children)
                    {
                        Console.Write(" " + edge.edgeValue);
                    }
                }
                else
                {
                    Console.Write("\n- {0} doesn't depend on any packages.", node.nodeValue.fileName);
                }
            }
        }

        // ----< display results of strong components >-----------------------
        static public void showStrongComponents(List<List<CsNode<FileNode, string>>> StrongComponents)
        {
            Console.WriteLine("\nStrong Components:");
            foreach (List<CsNode<FileNode, string>> scNodes in StrongComponents)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("- [").Append(scNodes[0].nodeValue.fileName);

                for (int i = 1; i < scNodes.Count; i++)
                {
                    stringBuilder.Append(", ").Append(scNodes[i].nodeValue.fileName);
                }
                stringBuilder.Append("]");

                Console.WriteLine(stringBuilder);
            }
        }


        //----< display a semiexpression on Console >--------------------

        static public void displaySemiString(string semi)
    {
      if (showSemi && useConsole)
      {
        Console.Write("\n");
        System.Text.StringBuilder sb = new StringBuilder();
        for (int i = 0; i < semi.Length; ++i)
          if (!semi[i].Equals('\n'))
            sb.Append(semi[i]);
        Console.Write("\n  {0}", sb.ToString());
      }
    }
    //----< display, possibly truncated, string >--------------------

    static public void displayString(Action<string> act, string str)
    {
      if (goSlow) Thread.Sleep(200);  //  here only to support visualization
      if (act != null && useFooter)
        act.Invoke(str.Truncate(width));
      if (useConsole)
        Console.Write("\n  {0}", str);
    }
    //----< display string, possibly overriding client pref >--------

    static public void displayString(string str, bool force=false)
    {
      if (useConsole || force)
        Console.Write("\n  {0}", str);
    }
    //----< display rules messages >---------------------------------

    static public void displayRules(Action<string> act, string msg)
    {
      if (showRules)
      {
        displayString(act, msg);
      }
    }
    //----< display actions messages >-------------------------------

    static public void displayActions(Action<string> act, string msg)
    {
      if (showActions)
      {
        displayString(act, msg);
      }
    }
    //----< display filename >---------------------------------------

    static public void displayFiles(Action<string> act, string file)
    {
      if (showFiles)
      {
        displayString(act, file);
      }
    }
    //----< display directory >--------------------------------------

    static public void displayDirectory(Action<string> act, string file)
    {
      if (showDirectories)
      {
        displayString(act, file);
      }
    }

#if(TEST_DISPLAY)
    static void Main(string[] args)
    {
      Console.Write("\n  Tested by use in Parser\n\n");
    }
#endif
  }
}
