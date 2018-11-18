/////////////////////////////////////////////////////////////////////
// Repository.cs -  A container is shared with all actions in      //
// ver 1.0          Code Analyzer.                                 //
//                                                                 //
// Yilin Ren,   CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains a Repository class which is a container 
 *   shared with all actions in Code Analyzer.
 * - The repository contains Scope Stack, Locations, TypeTable, UsingTable,
 *   NodeTable, AliasTable.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * ITokenCollection.cs
 * IRuleandAction.cs
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
using System.IO;
using CsGraph;
using Lexer;

namespace DepAnalysis
{
    using File = String;
    using UsingList = List<List<string>>;

    public struct FileNode
    {
        public string filePath { get; set; }
        public string fileName { get; set; }
        public FileNode(string filePath)
        {
            this.filePath = filePath;
            this.fileName = Path.GetFileName(filePath);
        }
    }

    public class Repository
    {
        ScopeStack<TypeElement> stack_ = new ScopeStack<TypeElement>();
        List<TypeElement> locations_ = new List<TypeElement>();
        TypeTable typeTable_ = new TypeTable();
        CsGraph<FileNode, string> depGraph_ =
            new CsGraph<FileNode, string>("DependencyGraph");
        Dictionary<File, UsingList> usingTable_ = new Dictionary<File, UsingList>();
        Dictionary<string, CsNode<FileNode, string>> nodeTable_ =
            new Dictionary<string, CsNode<FileNode, string>>();
        Dictionary<string, Dictionary<string, List<string>>> aliasTable_ =
            new Dictionary<string, Dictionary<string, List<string>>>();

        static Repository instance;

        public Repository()
        {
            instance = this;
        }

        //----< provides all code access to Repository >-------------------

        public static Repository getInstance()
        {
            return instance;
        }

        public void clearScopeStack()
        {
            stack_.clear();
        }

        //----< provides all actions access to current semiExp >-----------

        public ITokenCollection semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount(); }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }

        //----< enables recursively tracking entry and exit from scopes >--

        public int scopeCount
        {
            get;
            set;
        }

        public ScopeStack<TypeElement> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }

        // the locations table is the result returned by parser's actions
        // in this demo

        public List<TypeElement> locations
        {
            get { return locations_; }
            set { locations_ = value; }
        }

        public TypeTable typeTable
        {
            get { return typeTable_; }
            set { typeTable_ = value; }
        }

        public Dictionary<File, UsingList> usingTable
        {
            get { return usingTable_; }
            set { usingTable_ = value; }
        }

        public CsGraph<FileNode, string> depGraph
        {
            get { return depGraph_; }
            set { depGraph_ = value; }
        }

        public Dictionary<string, CsNode<FileNode, string>> nodeTable
        {
            get { return nodeTable_; }
            set { nodeTable_ = value; }
        }

        public Dictionary<string, Dictionary<string, List<string>>> aliasTable
        {
            get { return aliasTable_; }
            set { aliasTable_ = value; }
        }
#if (TEST_REPOSITORY)
        public static void Main(string[] args)
        {
            Repository repo = new Repository();
            Repository repo1 = Repository.getInstance();
            if(repo == repo1)
                Console.WriteLine("They are the same.");
        }
#endif
    }
}
