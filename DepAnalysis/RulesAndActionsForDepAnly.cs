/////////////////////////////////////////////////////////////////////
// RulesAndActionsForDepAnly.cs -  Rules and actions for building  //
// ver 1.0                         DepAnalyser                     //
//                                                                 //
// Yilin Ren,   CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - This package contains rules, including DetectUsingType, DetectType,
 *   DetectInheritType, and actions including AddDependency and AddUsingPkgs.
 * - DetectUsingType and AddUsingPkgs are responsible for detecting the used 
 *   namespaces and adding them into UsingTable.
 * - DetectInheritType, DetectType are responsible for detecting the possible 
 *   type name ,searching them in the TypeTable and creating dependency graph.
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

using Lexer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DepAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // rule to dectect using definitions

    public class DetectUsingType : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find("using", out index);
            int indexEqual;
            semi.find("=", out indexEqual);

            if (index != -1 && semi.size() > index + 1)
            {
                if (semi[semi.size() - 1] == ";" && indexEqual == -1
                    && semi[index + 1] != "System")
                {
                    ITokenCollection local = Factory.create();
                    local.filePath = semi.filePath;

                    for (int i = index + 1; i < semi.size() - 1; i++)
                        if (semi[i] != ".")
                            local.add(semi[i]);
                    doActions(local);

                }
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect possible type name

    public class DetectType : ARule
    {
        static string[] keywordsArray = new string[] { "abstract", "as", "base", "bool",
            "break","byte", "case", "catch","char","checked","class","const",
            "continue","decimal","default","delegate","do","double","else","enum",
            "event","explicit","extern","false","finally","fixed","float","for",
            "foreach","goto","if","implicit","in","int","interface","internal",
            "is","lock","long","namespace","new","null","object","operator","out",
            "override","params","private","protected","public","readonly","ref",
            "return","sbyte","sealed","short","sizeof","stackalloc","static","string",
            "struct","switch","this","throw","true","try","typeof","uint",
            "ulong","unchecked","unsafe","ushort","virtual","using",
            "void","volatile","while"};
        Dictionary<string, string> keywordsDic = keywordsArray.ToDictionary(key => key, value => value);


        public bool isPossibleType(string token)
        {
            return (char.IsLetterOrDigit(token[0]) || (token[0] == '_'))
                    && !isKeywords(token);
        }

        public bool isKeywords(string token)
        {
            return keywordsDic.ContainsKey(token);
        }

        public override bool test(ITokenCollection semi)
        {
            for (int i = 0; i < semi.size(); i++)
            {
                if (this.isPossibleType(semi[i]))
                {
                    ITokenCollection local = Factory.create();
                    local.filePath = semi.filePath;

                    int index = i;
                    for (int j = i - 1; j >= 0; j = j - 2)
                    {
                        if (semi[j] != ".")
                        {
                            index = j + 1;
                            break;
                        }
                        index = j - 1;
                    }

                    for (int k = index; k <= i; k = k + 2)
                        local.add(semi[k]);
                    doActions(local);
                }
            }
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect inherit definitions

    public class DetectInheritType : ARule
    {
        public bool isPossibleType(string token)
        {
            return (char.IsLetterOrDigit(token[0]) || (token[0] == '_'));
        }

        public override bool test(ITokenCollection semi)
        {
            int index = indexOfType(semi);
            int indexColon;
            semi.find(":", out indexColon);
            if (index != -1 && semi.size() > index + 1)
            {
                if (indexColon != -1 && semi.size() > indexColon + 1)
                {
                    for (int i = indexColon + 1; i < semi.size() - 1; i++)
                    {
                        if (this.isPossibleType(semi[i]))
                        {
                            ITokenCollection local = Factory.create();
                            local.filePath = semi.filePath;

                            int indexBegin = i;
                            for (int j = i - 1; j >= 0; j = j - 2)
                            {
                                if (semi[j] != ".")
                                {
                                    indexBegin = j + 1;
                                    break;
                                }
                                indexBegin = j - 1;
                            }

                            for (int k = indexBegin; k <= i; k = k + 2)
                                local.add(semi[k]);
                            doActions(local);
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // action to check the dependency among packages and build dependency graph

    public class AddDependency : AAction
    {
        public AddDependency(Repository repo)
        {
            repo_ = repo;
        }

        public List<string> aliasToFullName(ITokenCollection semi)
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < semi.size(); i++)
            {
                if (repo_.aliasTable.ContainsKey(semi.filePath)
                    && repo_.aliasTable[semi.filePath].ContainsKey(semi[i]))
                {
                    ret.AddRange(repo_.aliasTable[semi.filePath][semi[i]]);
                }
                else
                {
                    ret.Add(semi[i]);
                }
            }
            return ret;
        }

        public override void doAction(ITokenCollection semi)
        {
            List<string> oriSemi = aliasToFullName(semi);

            List<List<string>> nspaceCandidates = new List<List<string>>();
            List<string> scopeList = new List<string>();
            for (int i = 0; i < repo_.stack.count - 1; i++)
            {
                if (repo_.stack[i].type == "namespace")
                    scopeList.Add(repo_.stack[i].name);
            }
            nspaceCandidates.Insert(0, scopeList);

            if (repo_.usingTable.ContainsKey(semi.filePath))
            {
                List<List<string>> usingClone = new List<List<string>>();
                foreach (List<string> usingList in repo_.usingTable[semi.filePath])
                {
                    var newUsingList = new List<string>(usingList.Select(x => x.Clone() as string));
                    usingClone.Add(newUsingList);
                }

                nspaceCandidates.AddRange(usingClone);
            }

            List<string> nestedScope = new List<string>();
            for (int i = 0; i < oriSemi.Count - 1; i++)
            {
                nestedScope.Add(oriSemi[i]);    //
            }

            foreach (List<string> nsCnd in nspaceCandidates)
            {
                nsCnd.AddRange(nestedScope);
            }

            if (nestedScope.Count != 0)
                nspaceCandidates.Add(nestedScope);

            TypeElement typeElement = repo_.typeTable.findType(oriSemi[oriSemi.Count - 1], nspaceCandidates);  //

            if (typeElement != null && typeElement.filePath != semi.filePath)
            {
                repo_.nodeTable[semi.filePath].addChild(repo_.nodeTable[typeElement.filePath], typeElement.fileName);
                //Console.WriteLine("\n" + semi);
                //Console.WriteLine(System.IO.Path.GetFileName(semi.filePath) + " depends on " + typeElement.fileName);
            }

        }
    }

    ///////////////////////////////////////////////////////////////////
    // action to add the used namespaces into UsingTable

    public class AddUsingPkgs : AAction
    {
        public AddUsingPkgs(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            List<string> scopeList = new List<string>();
            for (int i = 0; i < semi.size(); i++)
            {
                scopeList.Add(semi[i]);
            }

            if (repo_.usingTable.ContainsKey(semi.filePath))
            {
                repo_.usingTable[semi.filePath].Add(scopeList);
            }
            else
            {
                List<List<string>> usingList = new List<List<string>>() { scopeList };
                repo_.usingTable.Add(semi.filePath, usingList);
            }
        }
    }


#if (TEST_RulesAndActionsForDepAnly)
    class RulesAndActionsForDepAnly
    {
        public static void Main(string[] args)
        {
             Console.Write("\n  Tested by use in DepAnalysis.\n\n");
        }
    }
#endif
}