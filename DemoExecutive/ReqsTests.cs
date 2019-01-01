/////////////////////////////////////////////////////////////////////
// ReqsTests.cs - an automated unit test that demonstrates         //
//                requirements                                     //
// ver 1.0                                                         //
// Yilin Ren, CSE681, Fall 2018                                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ----------------------
 * This package provides an automated unit test suite that demonstrates all 
 * of the functional requirements.
 * 
 * Required Files:
 * ---------------
 * - TestHarness.cs
 * - MainWindow.xaml.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 3 Dec 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.IO;
using Lexer;

namespace DepAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // utilities for display
    class ReqDisplay
    {
        //--------------------< display title >----------------------------------
        public static void title(string tle)
        {
            Console.Write("\n  {0}", tle);
            Console.Write("\n {0}", new string('-', tle.Length + 2));
        }

        //--------------------< display message >--------------------------------
        public static void message(string msg)
        {
            Console.Write("\n  {0}\n", msg);
        }

        //--------------------< display set >-------------------------------------
        public static void showSet(HashSet<string> set, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  Set:\n  ");
            foreach (var tok in set)
            {
                Console.Write("\"{0}\" ", tok);
            }
            Console.Write("\n");
        }

        //--------------------< display list >------------------------------------
        public static void showList(List<string> lst, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  List:\n  ");
            int count = 0;
            foreach (var tok in lst)
            {
                Console.Write("\"{0}\" ", tok);
                if (++count == 10)
                {
                    count = 0;
                    Console.Write("\n  ");
                }
            }
            Console.Write("\n");
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 3 
    class ReqTest3 : ITest
    {
        public string name { get; set; } = "Req3";
        public string path { get; set; } = "../../";
        public bool result { get; set; } = false;
        void onFile(string filename)
        {
            Console.Write("\n    {0}", filename);
            result = true;
        }
        void onDir(string dirname)
        {
            Console.Write("\n  {0}", dirname);
        }
        public bool doTest()
        {
            ReqDisplay.title("Req #3 - C# packages requirements");
            ReqDisplay.message("- Shall have C# packages: " +
                               "\n    Toker, SemiExp, TypeTable, TypeAnalysis," +
                               " DepAnalysis, " +
                               "\n    StrongComponent, Display, Tester." +
                               "\n  - This program including the following packages:");
            FileUtilities.Navigate nav = new FileUtilities.Navigate();
            nav.Add("*.cs");
            //nav.newDir += new FileUtilities.Navigate.newDirHandler(onDir);
            nav.newFile += new FileUtilities.Navigate.newFileHandler(onFile);
            List<string> listDirectory = new List<string>{ "LexicalScanner",
            "TypeTable", "TypeAnalysis", "DepAnalysis","StrongComponentAlys", "Display",
            "Repository", "DemoExecutive", "Graph", "FileMgr", "TestHarness" };
            foreach (string directory in listDirectory)
            {
                path = "../../../" + directory;
                nav.go(path, false);
            }

            Console.WriteLine();
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 4
    class ReqTest4 : ITest
    {
        public string name { get; set; } = "Req4";
        public string path { get; set; } = "../../";
        public bool result { get; set; } = false;
        public bool doTest()
        {
            Console.WriteLine();
            ReqDisplay.title("Req #4 - Support specifying all C# files in a sub-directory");
            ReqDisplay.message("- This program supports specifying the collection as all C# files " +
                               "\n    in a sub-directory tree, rooted at a specified path." +
                               "\n  - usage:  [a specified directory path]");


            string[] args = new string[] { "../../../DemoExecutive/Test/" };
            DemoExecutive.ShowCommandLine(args);
            List<string> files = DemoExecutive.ProcessCommandline(args);

            foreach (string file in files)
            {
                Console.Write("\n  Processing file {0}", Path.GetFileName(file));
            }

            Console.WriteLine();
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 5
    class ReqTest5 : ITest
    {
        public string name { get; set; } = "Req5";
        public string path { get; set; } = "../../";
        public bool result { get; set; } = false;
        public bool doTest()
        {
            Console.WriteLine();
            ReqDisplay.title("Req #5 - Identify the user-defined types in the specified set of files");
            ReqDisplay.message("- This program could dentify all of the Types defined within that code, " +
                               "\n    e.g., interfaces, classes, structs, enums, and delegates.");


            string[] args = new string[] { "../../../DemoExecutive/Test/" };
            DemoExecutive.ShowCommandLine(args);
            List<string> files = DemoExecutive.ProcessCommandline(args);

            Repository repo = new Repository();
            repo.semi = Factory.create();

            DemoExecutive.BuildTypeTable(args, files, repo);

            Display.showTypeTable(repo.typeTable);
            Console.WriteLine();
            Display.showAliasTable(repo.aliasTable);
            Console.WriteLine();
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 6 
    class ReqTest6 : ITest
    {
        public string name { get; set; } = "Req6";
        public string path { get; set; } = "../../";
        public bool result { get; set; } = false;
        public bool doTest()
        {
            Console.WriteLine();
            ReqDisplay.title("Req #6 - Find all strong components in the file collection");
            ReqDisplay.message("- In this program, Tarjan Algorithm is used to solve strong components.");

            string[] args = new string[] { "../../../DemoExecutive/Test/" };
            DemoExecutive.ShowCommandLine(args);
            List<string> files = DemoExecutive.ProcessCommandline(args);

            Repository repo = new Repository();
            repo.semi = Factory.create();

            BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
            Parser parser = builder.build();

            DemoExecutive.BuildTypeTable(args, files, repo);
            DemoExecutive.depAnalysis(args, files, repo);

            Console.Write("\n\nDependency Analysis:");
            Display.showDependency(repo.depGraph);

            Console.Write("\n");
            var strongComponents = TarjanSccSolver.DetectCycle(repo.depGraph);
            Display.showStrongComponents(strongComponents);

            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 7
    class ReqTest7 : ITest
    {
        public string name { get; set; } = "Req7";
        public string path { get; set; } = "../../";
        public bool result { get; set; } = false;
        public bool doTest()
        {
            Console.WriteLine();
            ReqDisplay.title("Req #7 - Display the results in a well formated area of the output");
            ReqDisplay.message("- It can be demonstrated by the result from Requirement 8.");
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // a test that demonstrates requirment 8
    class ReqTest8 : ITest
    {
        public string name { get; set; } = "Req8";
        public string path { get; set; } = "../../";
        static string message = "- build an automated unit test suite that demonstrates the requirements " +
                               "\n    and exercises all of the special cases." +
                               "\n  - Special cases includes: " +
                               "\n     * Implementing multiple interface. e.g:" +
                               "\n         public class C1 : Interface1, Interface2 { ..." +
                               "\n\n     * Nested namespaces. e.g:" +
                               "\n         namespace N1 " +
                               "\n         {" +
                               "\n             namespace N2 " +
                               "\n             {" +
                               "\n\n             }" +
                               "\n         }" +
                               "\n\n     * Internal Class. e.g:" +
                               "\n         namespace N1 " +
                               "\n         {" +
                               "\n             class C1 " +
                               "\n             {" +
                               "\n                class C2" +
                               "\n                {" +
                               "\n\n                } " +
                               "\n             }" +
                               "\n         }" +
                               "\n\n     * Using fully qualified name. e.g:" +
                               "\n         N1.N2.C1 c1 = new N1.N2.C1();" +
                               "\n\n     * Alias. e.g:" +
                               "\n         using A1 = N1.N2;" +
                                "\n         A1.C1 c1 = new A1.C1();" +
                               "\n\n     * Two or more classes with the same names that reside in different namespaces.";
        public bool result { get; set; } = false;
        public bool doTest()
        {
            Console.WriteLine();
            ReqDisplay.title("Req #8 - An automated unit test suite");
            ReqDisplay.message(message);
            string[] args = new string[] { "../../../DemoExecutive/SpecialTestCases/" };
            DemoExecutive.ShowCommandLine(args);
            List<string> files = DemoExecutive.ProcessCommandline(args);
            Repository repo = new Repository();
            repo.semi = Factory.create();
            DemoExecutive.BuildTypeTable(args, files, repo);
            Display.showTypeTable(repo.typeTable);
            Console.WriteLine();
            Display.showAliasTable(repo.aliasTable);
            Console.WriteLine();
            DemoExecutive.depAnalysis(args, files, repo);
            Console.Write("\n\nDependency Analysis:");
            Display.showDependency(repo.depGraph);
            Console.Write("\n");
            var strongComponents = TarjanSccSolver.DetectCycle(repo.depGraph);
            Display.showStrongComponents(strongComponents);
            return true;
        }
    }
}