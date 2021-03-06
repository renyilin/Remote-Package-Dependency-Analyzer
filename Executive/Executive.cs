﻿///////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Prototype Code Analyzer                //
// ver 1.0                                                           //
// Application:  CSE681, Project #3, Fall 2018                       //
// Author:      Yilin Ren                                            //
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
 * ver 1.0 : 03 Dec 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGraph;
using Lexer;

namespace DepAnalysis
{

    ///////////////////////////////////////////////////////////////////
    // Execute Dependency Analyzer
   
    class Executive
    {
        static string resultFilePath = ".\\result.txt";

        //---------< Build Type Table >------------------------------------------------------
        public static void BuildTypeTable(string[] args, List<string> files, Repository repo)
        {
            BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
            Parser parser = builder.build();

            foreach (string file in files)
            {
                //Console.Write("\n  Processing file {0}", System.IO.Path.GetFileName(file));
                if (file.Contains("TemporaryGeneratedFile") || file.Contains("AssemblyInfo"))
                    continue;

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

        //---------< Load files by processing command lines >-----------------------------------------
        public static List<string> ProcessCommandline(string[] args, bool optionFileRecursion)
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
            files = findFiles(path, "*.cs", optionFileRecursion);

            return files;
        }

        //---------< Find files with/w.o recursion >-----------------------------------------

        public static List<string> findFiles(string searchpath, string pattern, bool optionFileRecursion)
        {
            List<string> fileList = Directory.GetFiles(searchpath, pattern).ToList();
            if (optionFileRecursion)
            {
                foreach (string dir in Directory.GetDirectories(searchpath))
                {
                    if (dir == "." || dir == "..")
                        continue;
                    fileList.AddRange(findFiles(dir, pattern, optionFileRecursion));
                }
            }
            return fileList;
        }

        //---------< conduct dependency analysis >-----------------------------------------
        public static void depAnalysis(string[] args, List<string> files, Repository repo)
        {
            BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
            Parser parser = builder.build();

            BuildDepAnalyzer depBuilder = new BuildDepAnalyzer(repo);
            Parser depParser = depBuilder.build();

            foreach (string file in files)
            {
                if (file.Contains("TemporaryGeneratedFile") || file.Contains("AssemblyInfo"))
                    continue;

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

        //---------< output dependency >----------------------------------------------
        static public void showDependency(CsGraph<FileNode, string> DepGraph, StreamWriter streamWriter )
        {
            //Console.Write("\nDependency Analysis:");
            streamWriter.Write("\r\nDependency Analysis: ");

            foreach (CsNode<FileNode, string> node in DepGraph.adjList)
            {
                if (node.children.Count != 0)
                {
                    //Console.Write("\n- {0} depends on:", node.nodeValue.fileName);
                    streamWriter.Write("\r\n- {0} depends on:", node.nodeValue.fileName);
                    foreach (var edge in node.children)
                    {
                        //Console.Write(" " + edge.edgeValue);
                        streamWriter.Write(" " + edge.edgeValue);
                    }
                }
                else
                {
                    //Console.Write("\n- {0} doesn't depend on any packages.", node.nodeValue.fileName);
                    streamWriter.Write("\r\n- {0} doesn't depend on any packages.", node.nodeValue.fileName);
                }
            }
        }

        //---------< output strong components >-----------------------------------------
        static public void showStrongComponents(List<List<CsNode<FileNode, string>>> StrongComponents, StreamWriter streamWriter)
        {
            //Console.WriteLine("\nStrong Components:");
            streamWriter.WriteLine("\r\nStrong Components:");

            foreach (List<CsNode<FileNode, string>> scNodes in StrongComponents)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("- [").Append(scNodes[0].nodeValue.fileName);

                for (int i = 1; i < scNodes.Count; i++)
                {
                    stringBuilder.Append(", ").Append(scNodes[i].nodeValue.fileName);
                }
                stringBuilder.Append("]");

                //Console.WriteLine(stringBuilder);
                streamWriter.WriteLine(stringBuilder);
            }
        }

        //---------< Execute dependency analysis >-----------------------------------------
        static void Main(string[] args)
        {

            bool optionDA = Boolean.Parse(args[1]);
            bool optionSC = Boolean.Parse(args[2]);
            bool optionFileRecursion = Boolean.Parse(args[3]);

            using (StreamWriter streamWriter = new StreamWriter(resultFilePath))
            {
                streamWriter.WriteLine("Processing path: {0}", args[0]);

                List<string> files = ProcessCommandline(args, optionFileRecursion);
                Repository repo = new Repository();
                repo.semi = Factory.create();
                BuildTypeTable(args, files, repo);
                depAnalysis(args, files, repo);
                if (optionDA)
                {
                    showDependency(repo.depGraph, streamWriter);
                    streamWriter.WriteLine("");
                }

                if (optionSC)
                {
                    var strongComponents = TarjanSccSolver.DetectCycle(repo.depGraph);
                    showStrongComponents(strongComponents, streamWriter);
                }
            }

            //Console.Read();
        }
    }
}
