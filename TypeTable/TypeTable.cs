/////////////////////////////////////////////////////////////////////
// TypeTable.cs -  A container that saves type info including      //
// ver 1.0         type name, category and file path etc.          //
//                                                                 //
// Yilin Ren,   CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package concludes TypeElement and TypeTable class.
 * - TypeTable is a container that saves type info including type name, 
 *   category, file path, etc.
 * 
 * Required Files:
 * ---------------
 * Semi.cs
 * Toker.cs
 * ITokenCollection.cs
 * RulesAndActions.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepAnalysis
{
    using ScopeList = List<string>;
    using TypeName = String;

    ///////////////////////////////////////////////////////////////////
    // Type Element
    public class TypeElement
    {
        public string type { get; set; }
        public string name { get; set; }
        public int beginLine { get; set; }
        public int endLine { get; set; }
        public string filePath { get; set; }
        public string fileName { get; set; }
        public List<string> scopeList { get; set; }
        public List<TypeElement> dependency { get; set; }
        public List<TypeElement> strongComponents { get; set; }
        public int beginScopeCount { get; set; }
        public int endScopeCount { get; set; }


        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{").Append(scopeList[0]);
            for (int i = 1; i < scopeList.Count; i++)
                temp.Append(" : ").Append(String.Format("{0,-10}", scopeList[i]));
            temp.Append("}");
            return temp.ToString();
        }

    }

    ///////////////////////////////////////////////////////////////////
    // Type Table
    public class TypeTable
    {
        Dictionary<TypeName, List<TypeElement>> typeTable =
            new Dictionary<TypeName, List<TypeElement>>();

        //--------------< does the element exist in Dict's value >-----------------
        public bool isExistInValue(string typename, TypeElement elem)
        {
            foreach (var v in typeTable[typename])
                if (v.filePath == elem.filePath && v.scopeList.SequenceEqual(elem.scopeList))
                    return true;
            return false;
        }

        //--------------< add an element into the dictionary >-----------------
        public void add(TypeName typename, TypeElement elem)
        {
            if (typeTable.ContainsKey(typename))
            {
                if (!isExistInValue(typename, elem))
                    typeTable[typename].Add(elem);
            }
            else
            {
                List<TypeElement> temp = new List<TypeElement>();
                temp.Add(elem);
                typeTable.Add(typename, temp);
            }
        }

        //--------------< if a typename exists in the typetable >-----------------
        public bool containType(TypeName typename)
        {
            return typeTable.ContainsKey(typename);
        }

        public List<TypeElement> this[TypeName typename]
        {
            get
            {
                if (typeTable.ContainsKey(typename))
                    return typeTable[typename];
                else
                    return null;
            }
        }

        //--------------< output typetable >--------------------------------------
        public void show()
        {
            foreach (var elem in typeTable)
            {
                Console.Write("\n {0}", elem.Key);
                foreach (var item in elem.Value)
                {
                    Console.Write("\n    {0}", item.fileName);
                    Console.Write("\n   ");
                    foreach (string scope in item.scopeList)
                    {
                        Console.Write(" " + scope + " ");
                    }
                }
            }
            Console.Write("\n");
        }

        //--------------< find typename in typetable >----------------------------
        public TypeElement findType(TypeName typeName, List<List<string>> nspaceCandidates)
        {
            if (containType(typeName))
            {
                foreach (List<string> nspaceCandidate in nspaceCandidates)
                {
                    foreach (TypeElement typeElem in typeTable[typeName])
                    {
                        if (nspaceCandidate.SequenceEqual(typeElem.scopeList))
                        {
                            return typeElem;
                        }
                    }
                }
            }
            return null;
        }

        public List<List<TypeElement>> getTypeElements()
        {
            return typeTable.Values.ToList<List<TypeElement>>();
        }

#if(TEST_TYPETABLE)
        static void Main(string[] args)
        {
            TypeTable typeTable = new TypeTable();

            ScopeList scopeList1 = new List<string>() { "namespaceA" };
            ScopeList scopeList2 = new List<string>() { "namespaceB" };
            ScopeList scopeList3 = new List<string>() { "namespaceC" };
            TypeElement elem1 = new TypeElement() 
            {   type = "class", 
                name = "classA",
                fileName = "FileA" , 
                scopeList = scopeList1
            };
            TypeElement elem2 = new TypeElement() 
            {   type = "class", 
                name = "classB", 
                fileName = "FileB" ,
                scopeList = scopeList2
            };
            TypeElement elem3 = new TypeElement()
            {
                type = "class",
                name = "classB",
                fileName = "FileB",
                scopeList = scopeList2
            };
            typeTable.add("classA", elem1);
            typeTable.add("classB", elem2);
            typeTable.add("classA", elem3);

            typeTable.show();

            List<ScopeList> nspaceCandidates = new List<ScopeList>();
            nspaceCandidates.Add(new ScopeList() { "namespaceB" });
            nspaceCandidates.Add(new ScopeList() { "namespaceA" });
            TypeElement ret = typeTable.findType("classA", nspaceCandidates);

            Console.WriteLine();
            Console.WriteLine("Find classA (candidates is {{namespaceB}, {namespaceA}})");
            if (ret != null)
            {
                foreach(string scope in ret.scopeList)
                    Console.WriteLine(scope);
            }
            Console.Read();
        }
#endif
    }
}
