/////////////////////////////////////////////////////////////////////
// TokenFileSource.cs - Group tokens into grammatical sequences    //
//                                                                 //
// ver 1.0                                                         //
// Language:      C#, Visual Studio 10.0, .Net Framework 4.6.1     //
// Application:   Lexical Scanner, Project #2 for CSE681 - SMA     //
// Author:        Yilin Ren                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TokenFileSource provides, via class TokenSourceFile, facilities to read 
 * and peek characters from stream of files or code string.
 *
 * TokenSourceFile works with a character buffer charQ which could temporarily
 * store characters read from code file, so that it could provide peek() function
 * to peek character at specified position without removing it from the stream.
 * It also has a next() function that read the next character and remove it from
 * the stream.  
 *
 * Public Interface:
 * -----------------
 * public class TokenSourceFile : ITokenSource           // implement ITokenSource
 * TokenSourceFile tkSource = new TokenSourceFile()      // constructs a TokenSourceFile
 * if(tkSource.open(path))                               // attempt to open file
 * tkSource.openString(string)                           // attempt to load string
 * tkSource.close()                                      // close the source file
 * char chr = tkSource.next()                            // read and remove next character from stream
 * int nextIterm = tkSource.peek(int)                    // read the next n-th character without removing it 
 * if(end())                                             // reached the end of the stream?
 * 
 * Required Files:
 * ---------------
 * Tokenizer.cs
 * TokenFileSource.cs
 *
 * Maintenance History
 * -------------------
 * ver 1.0 : 30 Sep 2018
 * - first release
 */


using System;
using System.Collections.Generic;

namespace Lexer
{
    ///////////////////////////////////////////////////////////////////
    // ITokenSource interface
    // - Declares operations expected of any source of tokens
    // Author: Dr. Jim Fawcett

    public interface ITokenSource
    {
        bool open(string path);
        void close();
        int next();
        int peek(int n = 0);
        bool end();
        int lineCount { get; set; }
    }

    ///////////////////////////////////////////////////////////////////
    // TokenSourceFile class
    // - extracts integers from token source
    // - Streams often use terminators that can't be represented by
    //   a character, so we collect all elements as ints
    // - keeps track of the line number where a token is found
    // - uses StreamReader which correctly handles byte order mark
    //   characters and alternate text encodings.
    // - Modified by Yilin Ren, Based on demo from Dr. Jim Fawcett.

    public class TokenSourceFile : ITokenSource
    {
        public int lineCount { get; set; } = 1;
        private System.IO.TextReader fs_;           // physical source of text
        private List<int> charQ_ = new List<int>();   // enqueing ints but using as chars
        private TokenContext context_;

        public TokenSourceFile(TokenContext context)
        {
            context_ = context;
        }
        //----< attempt to open file with a System.IO.StreamReader >-----

        public bool open(string path)
        {
            try
            {
                fs_ = new System.IO.StreamReader(path, true);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}\n", ex.Message);
                return false;
            }
            return true;
        }

        //----< attempt to open string with a System.IO.StringReader >-----
        //- Yilin Ren

        public bool openString(string str)
        {
            try
            {
                fs_ = new System.IO.StringReader(str);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}\n", ex.Message);
                return false;
            }
            return true;
        }

        //----< close file >---------------------------------------------

        public void close()
        {
            fs_.Close();
        }
        //----< extract the next available integer >---------------------
        /*
         *  - checks to see if previously enqueued peeked ints are available
         *  - if not, reads from stream
         */
        public int next()
        {
            int ch;
            if (charQ_.Count == 0)  // no saved peeked ints
            {
                if (end())
                    return -1;
                ch = fs_.Read();
            }
            else                    // has saved peeked ints, so use the first
            {
                ch = charQ_[0];
                charQ_.Remove(ch);
            }
            if ((char)ch == '\n')   // track the number of newlines seen so far
                ++lineCount;

            return ch;
        }
        //----< peek n ints into source without extracting them >--------
        /*
         *  - This is an organizing prinicple that makes tokenizing easier
         *  - We enqueue because file streams only allow peeking at the first int
         *    and even that isn't always reliable if an error occurred.
         *  - When we look for two punctuator tokens, like ==, !=, etc. we want
         *    to detect their presence without removing them from the stream.
         *    Doing that is a small part of your work on this project.
         */
        public int peek(int n = 0)
        {
            if (n < charQ_.Count)  // already peeked, so return
            {
                return charQ_[n];
            }
            else                  // nth int not yet peeked
            {
                for (int i = charQ_.Count; i <= n; ++i)
                {
                    if (end())
                        return -1;
                    charQ_.Add(fs_.Read());  // read and enqueue
                }

                return charQ_[n];   // now return the last peeked
            }
        }
        //----< reached the end of the file stream? >-------------------------
        //- Modified by Yilin Ren 

        public bool end()
        {
            return fs_.Peek() < 0 && charQ_.Count == 0;
        }


#if (TEST_TOKENFILESOURCE)
   
        //----< Main - Test open(), openString(), peek(), next() >------------
        [STAThread]
        static void Main(string[] args)
        {
            TokenContext context_ = new TokenContext();
            TokenSourceFile src = new TokenSourceFile(context_);
            context_.src = src;
            string refFilePath = "../../Test2.txt";
            string testFilePath = System.IO.Path.GetFullPath(refFilePath);
            Console.WriteLine("\nProcessing " + testFilePath);
            src.open(refFilePath);

            for (int i = 0; i < 5; i++)
                Console.WriteLine("peek({0}): {1}", i, (char)src.peek(i));
            for (int i = 0; i < 5; i++)
                Console.WriteLine("next(): {0}", (char)src.next());
            src.close();

            // test openString()
            string str = "1234567890";
            Console.WriteLine("\nLoad String: " + str);
            src.openString(str);
            for (int i = 0; i < 5; i++)
                Console.WriteLine("peek({0}): {1}", i, (char)src.peek(i));
            for (int i = 0; i < 5; i++)
                Console.WriteLine("next(): {0}", (char)src.next());
            src.close();
        }

#endif

    }
}
