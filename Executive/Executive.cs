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
    class Executive
    {
        static string resultFilePath = ".\\result.txt";

        public static void BuildTypeTable(string[] args, List<string> files, Repository repo)
        {
            BuildTypeAnalyzer builder = new BuildTypeAnalyzer(repo);
            Parser parser = builder.build();

            foreach (string file in files)
            {
                //Console.Write("\n  Processing file {0}", System.IO.Path.GetFileName(file));

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
            files = findFiles(path, "*.cs");

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

        static public void showDependency(CsGraph<FileNode, string> DepGraph, StreamWriter streamWriter)
        {

            foreach (CsNode<FileNode, string> node in DepGraph.adjList)
            {
                if (node.children.Count != 0)
                {
                    Console.Write("\n- {0} depends on:", node.nodeValue.fileName);
                    streamWriter.Write("\r\n- {0} depends on:", node.nodeValue.fileName);
                    foreach (var edge in node.children)
                    {
                        Console.Write(" " + edge.edgeValue);
                        streamWriter.Write(" " + edge.edgeValue);
                    }
                }
                else
                {
                    Console.Write("\n- {0} doesn't depend on any packages.", node.nodeValue.fileName);
                    streamWriter.Write("\r\n- {0} doesn't depend on any packages.", node.nodeValue.fileName);
                }
            }
        }

        static public void showStrongComponents(List<List<CsNode<FileNode, string>>> StrongComponents, StreamWriter streamWriter)
        {
            Console.WriteLine("\nStrong Components:");
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

                Console.WriteLine(stringBuilder);
                streamWriter.WriteLine(stringBuilder);
            }
        }

        static void Main(string[] args)
        {           
            using (StreamWriter streamWriter = new StreamWriter(resultFilePath))
            {
                Console.WriteLine("Processing path: {0}", args[0]);
                streamWriter.WriteLine("Processing path: {0}", args[0]);

                List<string> files = ProcessCommandline(args);
                Repository repo = new Repository();
                repo.semi = Factory.create();
                BuildTypeTable(args, files, repo);
                //Display.showTypeTable(repo.typeTable);
                //Console.WriteLine();
                //Display.showAliasTable(repo.aliasTable);
                //Console.WriteLine();
                depAnalysis(args, files, repo);
                Console.Write("\nDependency Analysis:");
                streamWriter.Write("\r\nDependency Analysis: ");
                showDependency(repo.depGraph, streamWriter);
                Console.Write("\r\n");
                streamWriter.WriteLine("");
                var strongComponents = TarjanSccSolver.DetectCycle(repo.depGraph);
                showStrongComponents(strongComponents, streamWriter);
  
            }

            //Console.Read();
        }
    }
}
