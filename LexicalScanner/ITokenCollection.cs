/////////////////////////////////////////////////////////////////////
// ITokenCollection.cs - Interface for Token Collection            //
//                                                                 //
// ver 1.0                                                         //
// Language:      C#, Visual Studio 10.0, .Net Framework 4.6.1     //
// Application:   Lexical Scanner, Project #2 for CSE681 - SMA     //
// Author:        Yilin Ren, yren20@syr.edu                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * ITokenCollection is a interface for token collections. It Declares 
 * the operation and property expected of any token collection.
 * 
 * Public Interface:
 * -----------------
 * public class SemiExp : ITokenCollection    //implements ITokenCollection
 *
 * Required Files:
 * ---------------
 * ITokenCollection.cs
 *
 * Maintenance History
 * -------------------
 * ver 1.0 : 01 Oct 2018
 * - first release
 */

using System;
using System.Collections.Generic;

namespace Lexer
{
    ///////////////////////////////////////////////////////////////////
    // ITokenCollection Interface
    // - Declares the operation and property expected of any token collection. 

    using Token = String;
    using TokColl = List<String>;

    public interface ITokenCollection : IEnumerable<Token>
    {
        bool open(string source);                 // attach toker to source
        void close();                             // close toker's source
        TokColl get();                            // collect semi
        int size();                               // number of tokens
        Token this[int i] { get; set; }           // index semi
        ITokenCollection add(Token token);        // add a token to collection
        bool insert(int n, Token tok);            // insert tok at index n
        void clear();                             // clear all tokens
        bool contains(Token token);               // has token?
        bool find(Token tok, out int index);      // find tok if in semi
        Token predecessor(Token tok);             // find token before tok
        bool hasSequence(params Token[] tokSeq);  // does semi have this sequence of tokens?
        bool hasTerminator();                     // does semi have a valid terminator
        bool isDone();                            // at end of tokenSource?
        int lineCount();                          // get number of lines processed
        string ToString();                        // concatenate tokens with intervening spaces
        void show();                              // display semi on console
        string filePath { get; set; }
        void reset();
    }
#if (TEST_ITokenCollection)

    class TokenCollection : ITokenCollection
    {
        public List<string> semiExp { get; set; } = new List<string>();

        //-----< get token collection >---------------------------------------
        public List<string> get()
        {
            semiExp.Add("newToken");
            return semiExp;
        }

        //-----< Main >-------------------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {
            TokenCollection tokenCollection = new TokenCollection();
            tokenCollection.get();
            tokenCollection.get();
            foreach (string semi in tokenCollection.semiExp)
            {
                Console.WriteLine(semi);
            }
        }
    }
#endif
}
