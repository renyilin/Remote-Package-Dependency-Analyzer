/////////////////////////////////////////////////////////////////////
// StrongComponent.cs -  Find out strong components by             //
// ver 1.0               Tarjan Algorithm                          //
//                                                                 //
// Yilin Ren,   CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains a TarjanSccSolver class which is a container 
 *   shared with all actions in Code Analyzer.
 * - The repository contains Scope Stack, Locations, TypeTable, UsingTable,
 *   NodeTable, AliasTable.
 * 
 * Required Files:
 * ---------------
 * Graph.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.0 : 31 Oct 2018
 * - first release
 *
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGraph;

namespace DepAnalysis
{
    using Vertex = CsNode<FileNode, string>;


    public class TarjanSccSolver
    {
        private static List<List<Vertex>> StronglyConnectedComponents;
        private static Stack<Vertex> S;
        private static int index;
        private static CsGraph<FileNode, string> dg;

        public static List<List<Vertex>> DetectCycle(CsGraph<FileNode, string> graph)
        {
            StronglyConnectedComponents = new List<List<Vertex>>();
            index = 0;
            S = new Stack<Vertex>();
            dg = graph;
            foreach (Vertex v in graph.adjList)
            {
                if (v.Index < 0)
                {
                    strongconnect(v);
                }
            }
            return StronglyConnectedComponents;
        }

        private static void strongconnect(Vertex v)
        {
            v.Index = index;
            v.Lowlink = index;
            index++;
            S.Push(v);

            foreach (Vertex w in v.getChildrenNode())
            {
                if (w.Index < 0)
                {
                    strongconnect(w);
                    v.Lowlink = Math.Min(v.Lowlink, w.Lowlink);
                }
                else if (S.Contains(w))
                {
                    v.Lowlink = Math.Min(v.Lowlink, w.Index);
                }
            }

            if (v.Lowlink == v.Index)
            {
                List<Vertex> scc = new List<Vertex>();
                Vertex w;
                do
                {
                    w = S.Pop();
                    scc.Add(w);
                } while (v != w);
                StronglyConnectedComponents.Add(scc);
            }

        }
#if(TEST_STRONGCOMPONENT)

        public static void showStrongComponent(List<List<Vertex>> cycle_list)
        {
            Console.WriteLine("Strong Components:");
            foreach (List<Vertex> scNodes in cycle_list)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[").Append(scNodes[0].name);

                for (int i = 1; i < scNodes.Count; i++)
                {
                    stringBuilder.Append(",").Append(scNodes[i]);
                }
                stringBuilder.Append("]");

                Console.WriteLine(stringBuilder);
            }
        }


        public static void Main(string[] args)
        {
            CsGraph<FileNode, string> graph = new CsGraph<FileNode, string>("DepGraph");
            FileNode fileNode1 = new FileNode();
            fileNode1.fileName = "F1";
            FileNode fileNode2 = new FileNode();
            fileNode2.fileName = "F2";
            FileNode fileNode3 = new FileNode();
            fileNode3.fileName = "F3";
            FileNode fileNode4 = new FileNode();
            fileNode4.fileName = "F4";
            FileNode fileNode5 = new FileNode();
            fileNode5.fileName = "F5";
            FileNode fileNode6 = new FileNode();
            fileNode6.fileName = "F6";
            FileNode fileNode7 = new FileNode();
            fileNode7.fileName = "F7";
            FileNode fileNode8 = new FileNode();
            fileNode8.fileName = "F8";
            Vertex v1 = new Vertex("F1") { nodeValue = fileNode1 };
            Vertex v2 = new Vertex("F2") { nodeValue = fileNode2 };
            Vertex v3 = new Vertex("F3") { nodeValue = fileNode3 };
            Vertex v4 = new Vertex("F4") { nodeValue = fileNode4 };
            Vertex v5 = new Vertex("F5") { nodeValue = fileNode5 };
            Vertex v6 = new Vertex("F6") { nodeValue = fileNode6 };
            Vertex v7 = new Vertex("F7") { nodeValue = fileNode7 };
            Vertex v8 = new Vertex("F8") { nodeValue = fileNode8 };
            v1.addChild(v2, "edge12");
            v1.addChild(v5, "edge15");
            v2.addChild(v6, "edge26");
            v3.addChild(v4, "edge34");
            v3.addChild(v2, "edge32");
            v3.addChild(v7, "edge37");
            v4.addChild(v7, "edge47");
            v5.addChild(v1, "edge51");
            v5.addChild(v6, "edge56");
            v6.addChild(v3, "edge63");
            v6.addChild(v7, "edge67");
            v7.addChild(v8, "edge78");
            v8.addChild(v4, "edge84");
            graph.addNode(v1);graph.addNode(v2);
            graph.addNode(v3);graph.addNode(v4);
            graph.addNode(v5);graph.addNode(v6);
            graph.addNode(v7);graph.addNode(v8);
            var cycle_list = TarjanSccSolver.DetectCycle(graph);
            showStrongComponent(cycle_list);
        }
#endif

    }

}