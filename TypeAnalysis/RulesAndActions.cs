/////////////////////////////////////////////////////////////////////
// RulesAndActions.cs -  Rules and actions for building            //
// ver 1.0               Type Analyser                             //
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
using System.IO;
using Lexer;

namespace DepAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // Define Actions
    ///////////////////////////////////////////////////////////////////
    // - PushStack
    // - PopStack
    // - PrintFunction
    // - PrintSemi
    // - SaveDeclar
    // - AddTypeTable

    ///////////////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope
    // - pushes element with type and name onto stack
    // - records starting line number
    public class PushStack : AAction
    {
        public PushStack(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack");
            ++repo_.scopeCount;
            TypeElement elem = new TypeElement();
            elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
            elem.name = semi[1];     // expects name
            elem.beginLine = repo_.semi.lineCount() - 1;
            elem.endLine = 0;        // will be set by PopStack action
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = 0;  // will be set by PopStack action
            repo_.stack.push(elem);

            // display processing details if requested

            if (AAction.displayStack)
                repo_.stack.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }

            // add starting location if namespace, type, or function

            if (elem.type == "control" || elem.name == "anonymous")
                return;
            repo_.locations.Add(elem);
        }
    }


    ///////////////////////////////////////////////////////////////////
    // pushes scope info on stack and add class type info into TypeTable
    // - pushes element with type and name onto stack
    // - adds class type info into TypeTable
    public class AddTypeTable : AAction
    {
        public AddTypeTable(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack");

            if (semi[0] != "delegate")
                ++repo_.scopeCount;
            TypeElement elem = new TypeElement();
            elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
            if (elem.type != "delegate")
                elem.name = semi[1];     // expects name
            else
                elem.name = semi[2];     // expects name
            elem.beginLine = repo_.semi.lineCount() - 1;
            elem.endLine = 0;        // will be set by PopStack action
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = 0;  // will be set by PopStack action
            elem.filePath = semi.filePath;
            elem.fileName = Path.GetFileName(elem.filePath);

            repo_.stack.push(elem);

            // display processing details if requested

            if (AAction.displayStack)
                repo_.stack.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }

            // add starting location if namespace, type, or function

            List<string> scopeList = new List<string>();
            for (int i = 0; i < repo_.stack.count - 1; i++)
            {
                scopeList.Add(repo_.stack[i].name);
            }
            elem.scopeList = scopeList;

            if (elem.type != "namespace" && elem.type != "function"
                && elem.type != "control")
                repo_.typeTable.add(elem.name, elem);

            if (elem.type == "delegate")
                repo_.stack.pop();
        }
    }
    ///////////////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope
    // - records end line number and scope count

    public class PopStack : AAction
    {
        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            TypeElement elem;
            try
            {
                // if stack is empty (shouldn't be) pop() will throw exception

                elem = repo_.stack.pop();

                // record ending line count and scope level

                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    TypeElement temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).endLine == 0)
                            {
                                (repo_.locations[i]).endLine = repo_.semi.lineCount();
                                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            if (AAction.displaySemi)
            {
                Lexer.ITokenCollection local = Factory.create();
                local.add(elem.type).add(elem.name);
                if (local[0] == "control")
                    return;

                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }
    ///////////////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }
        public override void display(Lexer.ITokenCollection semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.size(); ++i)
            {
                if (semi[i] != "\n")
                    Console.Write("{0} ", semi[i]);
            }
        }
        public override void doAction(ITokenCollection semi)
        {
            this.display(semi);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // ITokenCollection printing action, useful for debugging

    public class PrintSemi : AAction
    {
        public PrintSemi(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
            this.display(semi);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // display public declaration

    public class SaveDeclar : AAction
    {
        public SaveDeclar(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            TypeElement elem = new TypeElement();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.beginLine = repo_.lineCount;
            elem.endLine = elem.beginLine;
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = elem.beginScopeCount;
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            repo_.locations.Add(elem);
        }
    }

    ///////////////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class AddAliasTable : AAction
    {
        public AddAliasTable(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            List<string> scopeList = new List<string>();
            for (int i = 1; i < semi.size(); i++)
            {
                scopeList.Add(semi[i]);
            }

            if (repo_.aliasTable.ContainsKey(semi.filePath))
            {
                if (!repo_.aliasTable[semi.filePath].ContainsKey(semi[0]))
                    repo_.aliasTable[semi.filePath].Add(semi[0], scopeList);
            }
            else
            {
                Dictionary<string, List<string>> aliasDic = new Dictionary<string, List<string>>();
                aliasDic.Add(semi[0], scopeList);
                repo_.aliasTable.Add(semi.filePath, aliasDic);
            }

        }
    }

    ///////////////////////////////////////////////////////////////////
    // Define Rules
    ///////////////////////////////////////////////////////////////////
    // - DetectNamespace
    // - DetectClass
    // - DetectFunction
    // - DetectAnonymousScope
    // - DetectPublicDeclaration
    // - DetectLeavingScope
    // - DetectAlias

    ///////////////////////////////////////////////////////////////////
    // rule to detect alias

    public class DetectAlias : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find("using", out index);
            int indexEqual;
            semi.find("=", out indexEqual);

            if (index != -1 && semi.size() > index + 1)
            {
                if (semi[semi.size() - 1] == ";"
                    && indexEqual != -1 && semi[index + 1] != "System")
                {
                    ITokenCollection local = Factory.create();
                    local.filePath = semi.filePath;

                    local.add(semi[indexEqual - 1]);
                    for (int i = indexEqual + 1; i < semi.size() - 1; i++)
                        if (semi[i] != ".")
                            local.add(semi[i]);
                    doActions(local);
                    return true;
                }

            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectNamespace");
            int index;
            semi.find("namespace", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectClass");
            int index = indexOfType(semi);

            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.filePath = semi.filePath;
                // local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect delegate definitions

    public class DetectDelegate : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectClass");
            int indexDG;
            semi.find("delegate", out indexDG);

            if (indexDG != -1 && semi.size() > indexDG + 1
                && semi[semi.size() - 1] == ";")
            {
                ITokenCollection local = Factory.create();
                local.filePath = semi.filePath;
                // local semiExp with tokens for type and name
                local.add(semi[indexDG]).add(semi[indexDG + 1]).add(semi[indexDG + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectFunction");
            if (semi[semi.size() - 1] != "{")
                return false;

            int index;
            semi.find("(", out index);
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                local.filePath = semi.filePath;
                local.add("function").add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those

    public class DetectAnonymousScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
            int index;
            semi.find("{", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                local.filePath = semi.filePath;
                // create local semiExp with tokens for type and name
                local.add("control").add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect public declaration

    public class DetectPublicDeclar : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectPublicDeclar");
            int index;
            semi.find(";", out index);
            if (index != -1)
            {
                semi.find("public", out index);
                if (index == -1)
                    return true;
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                //local.displayNewLines = false;
                local.add("public " + semi[index + 1]).add(semi[index + 2]);

                semi.find("=", out index);
                if (index != -1)
                {
                    doActions(local);
                    return true;
                }
                semi.find("(", out index);
                if (index == -1)
                {
                    doActions(local);
                    return true;
                }
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
            int index;
            semi.find("}", out index);
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // BuildCodeAnalyzer class
    ///////////////////////////////////////////////////////////////////

    public class BuildTypeAnalyzer
    {
        Repository repo = new Repository();

        public BuildTypeAnalyzer(Repository rep)//, Lexer.ITokenCollection semi)
        {
            repo = rep;
            // repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default

            AddAliasTable addAliasTable = new AddAliasTable(repo);

            // capture using info
            DetectAlias detectAlias = new DetectAlias();
            detectAlias.add(addAliasTable);
            parser.add(detectAlias);

            // action used for namespaces, classes, and functions
            AddTypeTable addTypeTable = new AddTypeTable(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(addTypeTable);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(addTypeTable);
            parser.add(detectCl);

            // capture delegate info
            DetectDelegate detectDG = new DetectDelegate();
            detectDG.add(addTypeTable);
            parser.add(detectDG);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(addTypeTable);
            parser.add(detectFN);

            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(addTypeTable);
            parser.add(anon);

            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            leave.add(pop);
            parser.add(leave);
            return parser;
        }

#if (TEST_RULESANDACTIONS)
        static void Main()
        {
            Console.WriteLine("This part will be tested in TypeAnalysis package.");
        }
#endif
    }
}
