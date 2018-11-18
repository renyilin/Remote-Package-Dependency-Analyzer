/////////////////////////////////////////////////////////////////////
// DepAnalysis.cs -  Build DepAnalyzer by rules and actions        //
// ver 1.0                                                         //
//                                                                 //
// Yilin Ren,   CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains a BuildDepAnalyzer class built by rules and
 *   actions.
 * - It can detect using info and analyze the type-based dependency among
 *   packages.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * ITokenCollection.cs
 * RulesAndActions.cs
 * DepAnalysis.cs
 * TypeAnalysis.cs
 * IRuleandAction.cs
 * TypeTable.cs
 * TypeElement.cs
 * Graph.cs
 * Display.cs
 * Repository.cs
 * ScopeStack.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 31 Oct 2018
 * - first release
 *
 */

using System;
using System.Collections.Generic;
using Lexer;
using CsGraph;

namespace DepAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // BuildDepAnalyzer class
    // - Build DepAnalyzer by rules and actions.  

    public class BuildDepAnalyzer
    {
        Repository repo = new Repository();

        public BuildDepAnalyzer(Repository rep)
        {
            repo = rep;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default

            // action used for namespaces, classes, and functions
            AddUsingPkgs addUsing = new AddUsingPkgs(repo);

            // capture using info
            DetectUsingType detectUsing = new DetectUsingType();
            detectUsing.add(addUsing);
            parser.add(detectUsing);

            // detect namspace
            DetectNamespace detectNS = new DetectNamespace();
            parser.add(detectNS);

            // detect closed curve bracket
            DetectLeavingScope detectLeaving = new DetectLeavingScope();
            parser.add(detectLeaving);

            AddDependency addDependency = new AddDependency(repo);

            // detect inherit classes or interfaces
            DetectInheritType detectInherit = new DetectInheritType();
            detectInherit.add(addDependency);
            parser.add(detectInherit);

            // detect typename, find dependency and add it into dependency graph.
            DetectType detectType = new DetectType();
            detectType.add(addDependency);
            parser.add(detectType);

            return parser;
        }

#if(TEST_DEPANALYSIS)
        public static void typeAnalyzer(Repository repo, List<string> files, string[] args)
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

            public static void DepAnalyzer(Repository repo, List<string> files, string[] args)
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
            Console.Write("\n  Demonstrating Parser");
            Console.Write("\n ======================\n");

            TestParser.ShowCommandLine(args);

            List<string> files = TestParser.ProcessCommandline(args);
            Repository repo = new Repository();
            repo.semi = Factory.create();

            typeAnalyzer(repo, files, args);

            Console.Write("\n\n  Type and Dependency Analysis");
            Console.Write("\n ----------------------------");

            TypeTable table = repo.typeTable;
            Display.showTypeTable(table);

            DepAnalyzer(repo, files, args);

            Console.Write("\n\nDependency Analysis:");
            Display.showDependency(repo.depGraph);

            Console.Write("\n");
            var strongComponents = TarjanSccSolver.DetectCycle(repo.depGraph);
            Display.showStrongComponents(strongComponents);

            Console.Write("\n\n");

        }
#endif
    }

}
