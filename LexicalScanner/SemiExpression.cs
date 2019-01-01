/////////////////////////////////////////////////////////////////////
// SemiExpression.cs - Group tokens into grammatical sequences     //
//                                                                 //
// ver 1.0                                                         //
// Language:      C#, Visual Studio 10.0, .Net Framework 4.6.1     //
// Application:   Lexical Scanner, Project #2 for CSE681 - SMA     //
// Author:        Yilin Ren, yren20@syr.edu                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Semi provides, via class SemiExp, facilities to extract semiExpressions.
 * A semiExpression is a collection of tokens that is just the right amount
 * of information to parse for code analysis. semiExpressions are determined
 * by special terminating characters: semicolon, open brace, closed brace,  
 * newline when preceeded on the same line with '#', etc.
 *
 * SemiExp works with a Toker object attached to a specified source code file.
 * SemiExp provides a get() function that extracts semiExpressions from the file
 * while filtering out comments.
 * 
 * Public Interface:
 * -----------------
 * SemiExp semi = new SemiExp();              // constructs SemiExp object
 * List<string> semiExp { get; set; } = new List<string>();  // creates a token collection
 * if(tk.openString("int i =0;"))             // attaches toker to specified string
 * tk.close()                                 // close file stream
 * Token token = tk.getTok()                  // get one token every calling
 * tk.lineCount()                             // count line
 * while(!isDone())                           // if Toker reached end of its source
 * List<string> get()                         // get one semiexpression
 * Token tok = semi[i]                        // get token through indexer
 * string str = DisplayStr()                  // combine tokens and add space between them
 *
 * Required Files:
 * ---------------
 * Tokenizer.cs
 * TokenFileSource.cs
 * SemiExpression.cs
 *
 * Maintenance History
 * -------------------
 * ver 1.0 : 30 Sep 2018
 * - first release
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lexer
{
    using Token = StringBuilder;

    static public class Factory
    {
        static public ITokenCollection create()
        {
            Semi rtn = new Semi();
            rtn.toker = new Toker();
            return rtn;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // SemiExp class
    // - applications need to use only this class to collect semi-expressions.
    // - Author: Yilin Ren. 
    // - refer to demo from Dr. Jim Fawcett

    public class Semi : ITokenCollection
    {
        public List<string> semiExp { get; set; } = null;
        public Toker toker { get; set; } = null;
        public string filePath { set; get; }
        Token currTok = new StringBuilder("");
        bool isEOF = false;

        //-----< Line Counter >------------------------------------------
        public int lineCount()
        {
            return toker.lineCount();
        } 

        //-----< Constructor >------------------------------------------
        public Semi()
        {
            semiExp = new List<string>();
            toker = new Toker();
        }

        //-----< Open Source File Stream >------------------------------
        public bool open(string filePath)
        {
            this.filePath = filePath;
            return toker.open(filePath);
        }

        //-----< Open Source String >-----------------------------------
        public bool openString(string str)
        {
            return toker.openString(str);
        }

        //-----< Close Source File Stream or String >-------------------
        public void close()
        {
            toker.close();
        }

        //-----< number of tokens >-------------------
        public int size()
        {
            return semiExp.Count;
        }

        //----< indexer allows us to index for a specific token >--------
        public String this[int i]
        {
            get { return semiExp[i]; }
            set { semiExp[i] = value; }
        }

        //----< add a token to the end of this semi-expression >---------
        public ITokenCollection add(String token)
        {
            semiExp.Add(token);
            return this;
        }

        //----< clear all the tokens from internal collection >----------
        public void clear()
        {
            semiExp.Clear();
        }

        //----< insert a token at position n >---------------------------
        public bool insert(int n, String tok)
        {
            if (n < 0 || n >= tok.Length)
                return false;
            semiExp.Insert(n, tok);
            return true;
        }

        //----< function returning an enumerator >-----------------------

        public IEnumerator<String> GetEnumerator()
        {
            return semiExp.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //----< does semi contain specific token? >----------------------

        public bool contains(String tok)
        {
            if (semiExp.Contains(tok))
                return true;
            return false;
        }

        //----< find token in semi >-------------------------------------

        public bool find(String tok, out int index)
        {
            for (index = 0; index < size(); ++index)
            {
                if (semiExp[index] == tok)
                    return true;
            }
            index = -1;
            return false;
        }

        //----< find predecessor of token >------------------------------

        public String predecessor(String tok)
        {
            int index;
            if (find(tok, out index) && index > 0)
            {
                return semiExp[index - 1];
            }
            return "";
        }

        //----< test for ordered sequence of tokens >--------------------

        public bool hasSequence(params String[] tokSeq)
        {
            int position = 0;
            foreach (var tok in semiExp)
            {
                if (position == tokSeq.Length - 1)
                    return true;
                if (tok == tokSeq[position])
                    ++position;
            }
            return (position == tokSeq.Length - 1);
        }

        //----< does this semi-expression contain a terminator? >--------
        public bool hasTerminator()
        {
            if (semiExp.Count <= 0)
                return false;
            if (isTerminator(semiExp[semiExp.Count - 1]))
                return true;
            return false;
        }

        //----< display contents of semi-expression on console >---------
        public void show()
        {
            Console.Write("\n-- ");
            foreach (string tok in semiExp)
            {
                if (tok != "\n")
                    Console.Write("{0} ", tok);
                else
                    Console.Write("{0} ", "\\n");
            }
        }

        //-----< Is it the end of file? >-------------------------------
        public bool isDone()
        {
            return isEOF;
        }

        public void reset()
        {
            toker.reset();
        }

        //-----< Is it the comment? >-----------------------------------
        bool isComment(Token token )
        {
            string currTokStr = currTok.ToString();
            return (currTokStr.StartsWith("//", StringComparison.Ordinal))
                    || (currTokStr.StartsWith("/*", StringComparison.Ordinal)
                    && currTokStr.EndsWith("*/", StringComparison.Ordinal));
        }

        //-----< Is this the last token in the current semiExp? >-------
        /*- There are six terminations:
         *   a) semicolon;
         *   b) open brace; 
         *   c) close brace;
         *   d) switch statement (begining with "case" or "default"
         *      and ending with ':')
         *   e) conditional compilation (begining with '#' and ending 
         *      with new line) 
         *   f) attributes (begining with '[' and ending with ']')
         */
        bool isTerminator(string tok)
        {
            switch (tok)
            {
                case ";": return true;
                case "{": return true;
                case "}": return true;
                case ":":                       //handle switch statement
                          if (semiExp.Count != 0 && (semiExp[0] == "case"
                              || semiExp[0] == "default"))
                          return true;
                          return false;
                case "\r":
                case "\n":  //handle the situation that starts with "#" ending with new line
                          if (semiExp.Count != 0 && semiExp[0] == "#")
                                return true;
                            return false;
                case "]":                       //handle attributes
                          if (semiExp.Count != 0 && semiExp[0] == "[")
                                return true;
                            return false;
                default: return false;
            }
        }

        //-----< Get next semiExpression by grouping tokens >------------
        //- return a semiExp every calling.
        //- ignore semicolons within parentheses in a for(;;) expression.
        public List<string> get()
        {
            int forSemicolonCount = 0;    // the number of the semicolons in the for statement. 
            semiExp.Clear();  // empty container

            while (true)
            {
                currTok = toker.getTok();
                if (currTok == null)   // end of file
                {
                    isEOF = true;
                    break;
                }
                string currTokStr = currTok.ToString();
                // when current token is not newline and comment, Add current token into semiExp.
                if (currTokStr != "\n" && currTokStr != "\r" 
                    && !isComment(currTok))  
                    semiExp.Add(currTokStr);   // Add current token into semiExp.
                if (currTokStr == "for")   
                    forSemicolonCount += 2;
                if (forSemicolonCount > 0 && currTokStr == ";")
                {
                    forSemicolonCount--;
                    continue;  //when it is semicolon in for(;;), continue the loop.
                }
                if (isTerminator(currTokStr))
                    break;
            }
            return semiExp;
        }

        //-----< Convert semiExp to string >-----------------------------
        //- combine tokens and add space between them.
        public string DisplayStr(List<string> semiExp)
        {
            StringBuilder disp = new StringBuilder("");
            foreach (string tok in semiExp)
            {
                disp.Append(tok);
                if (tok.IndexOf('\n') != tok.Length - 1)
                    disp.Append(" ");
            }
            return disp.ToString();
        }
    }

#if (TEST_SEMIEXP)
    public class TestSemiExpression
    {
        //-----< Semiexpression from souce code file >------------------------
        static bool testSemiExpression(string path)
        {
            //Toker toker = new Toker();
            SemiExp semiExp = new SemiExp();


            string fqf = System.IO.Path.GetFullPath(path);
            if (!semiExp.open(fqf))
            {
                Console.Write("\n can't open {0}\n", fqf);
                return false;
            }
            else
            {
                Console.Write("\n  SemiExpressions:");
                Console.Write("\n  ================");
            }
            while (!semiExp.isDone())
            {
                List<String> sec = semiExp.get();
                if (sec.Count != 0)
                    Console.Write("\n  - line{0, 3} : {1}", semiExp.lineCount, semiExp.DisplayStr(sec));
            }
            Console.Write("\n");
            semiExp.close();
            return true;
        }

        [STAThread]
        static void Main()
        {
            string refFilePath = "../../Test.txt";
            string testFilePath = System.IO.Path.GetFullPath(refFilePath);
            Console.WriteLine("\n  Processing " + testFilePath);
            testSemiExpression(refFilePath);
        }
    }
#endif

}
