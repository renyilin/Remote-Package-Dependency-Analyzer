/////////////////////////////////////////////////////////////////////
// Tokenizer.cs - Reads words and punctuation symbols              //
//                from a source code file                          //
// ver 1.0                                                         //
// Language:      C#, Visual Studio 10.0, .Net Framework 4.6.1     //
// Application:   Lexical Scanner, Project #2 for CSE681 - SMA     //
// Author:        Yilin Ren, yren20@syr.edu                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Tokenizer provides the facilities to tokenize ASCII text files
 * via the class Toker. Specifically, it divides the file's stream into 
 * tokens including words, punctuations, comments, etc. 
 * 
 * Tokenizer implements state pattern design. It can deal with various 
 * kinds of tokens. Toker::getTok() could produces one token for each call.
 *
 * Public Interface:
 * -----------------
 * Toker tk = new Toker();                // constructs Toker object
 * if(tk.open("../abc.cs"))...            // attaches toker to specified file
 * if(tk.openString("int i =0"))          // attaches toker to specified string
 * tk.close()                             // close file stream
 * Token token = tk.getTok()              // get one token every calling
 * tk.lineCount()                         // count line
 * while(!isDone())                       // if Toker reached end of its source
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
using System.Text;

namespace Lexer
{
    using Token = StringBuilder;

    ///////////////////////////////////////////////////////////////////
    // TokenContext class
    // - holds all the tokenizer states
    // - holds source of tokens
    // - internal qualification limits access to this assembly

    ///////////////////////////////////////////////////////////////////
    // ITokenState interface
    // - Declares operations expected of any token gathering state
    // - Author: Dr. Jim Fawcett 

    public interface ITokenState
    {
        Token getTok();
        bool isDone();
    }

    ///////////////////////////////////////////////////////////////////
    // Toker class
    // - applications need to use only this class to collect tokens
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    public class Toker
    {
        private TokenContext context_;       // holds single instance of all states and token source
        public string path { get; set; }

        //----< initialize state machine >-------------------------------

        public Toker()
        {
            context_ = new TokenContext();      // context is the glue that holds all of the state machine parts 
        }
        //----< attempt to open source of tokens >-----------------------
        /*
         * If src is successfully opened, it uses TokenState.nextState(context_)
         * to set the initial state, based on the source content.
         */
        public bool open(string path)
        {
            TokenSourceFile src = new TokenSourceFile(context_);
            context_.src = src;
            this.path = path;
            return src.open(path);
        }

        //----< attempt to open string source >--------------------------

        public bool openString(string str)
        {
            TokenSourceFile src = new TokenSourceFile(context_);
            context_.src = src;
            return src.openString(str);
        }

        //----< close source of tokens >---------------------------------

        public void close()
        {
            context_.src.close();
        }

        ////----< extract a token from source >----------------------------
        /// - Modified by Yilin Ren.
        public Token getTok()
        {
            Token tok = null;
            while (!isDone())
            {
                tok = context_.currentState_.getTok();
                if (tok != null)       // When it's not InitialState
                {
                    context_.currentState_ = context_.bs_;
                    if (!context_.ws_.isWhiteSpaceStart(tok[0]))
                        break;
                }
            }
            return tok;
        }

        //----< Reset initial state >-------------------
        public void reset()
        {
            context_.currentState_ = context_.bs_;  //Reset
        }

        //----< has Toker reached end of its source? >-------------------
        public bool isDone()
        {
            if (context_.currentState_ == null)
            {
                reset(); //reset for next time
                return true;
            }
            return context_.currentState_.isDone();
        }
        public int lineCount() { return context_.src.lineCount; }
    }
    ///////////////////////////////////////////////////////////////////
    // TokenContext class
    // - holds all the tokenizer states
    // - holds source of tokens
    // - internal qualification limits access to this assembly
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    public class TokenContext
    {
        internal TokenContext()
        {
            ws_ = new WhiteSpaceState(this);
            ps_ = new PunctState(this);
            as_ = new AlphaState(this);
            bs_ = new InitialState(this);
            ccs_ = new CCommentState(this);
            cppcs_ = new CppCommentState(this);
            dcsps_ = new DoubleCharSpecialPunctState(this);
            scsp_ = new SingleCharSpecialPunctState(this);
            dqs_ = new DoubleQuoteState(this);
            sqs_ = new SingleQuoteState(this);
            ns_ = new NewlineState(this);

            // Initail state is BaseSate.
            currentState_ = bs_;
        }
        internal WhiteSpaceState ws_ { get; set; }
        internal PunctState ps_ { get; set; }
        internal AlphaState as_ { get; set; }
        internal InitialState bs_ { get; set; }  // ----YILIN REN
        internal CCommentState ccs_ { get; set; }
        internal CppCommentState cppcs_ { get; set; }
        internal DoubleCharSpecialPunctState dcsps_ { get; set; }
        internal SingleCharSpecialPunctState scsp_ { get; set; }
        internal DoubleQuoteState dqs_ { get; set; }
        internal SingleQuoteState sqs_ { get; set; }
        internal NewlineState ns_ { get; set; }
        internal TokenState currentState_ { get; set; }

        internal ITokenSource src { get; set; }  // can hold any derived class
    }

    ///////////////////////////////////////////////////////////////////
    // TokenState class
    // - base for all the tokenizer states
    // - nextState() is responsible for changing current state when
    //   some state's start condition is satisfied.
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    abstract class TokenState : ITokenState
    {

        internal TokenContext context_ { get; set; }  // derived classes store context ref here

        //----< delegate source opening to context's src >---------------

        public bool open(string path)
        {
            return context_.src.open(path);
        }
        //----< pass interface's requirement onto derived states >-------

        public abstract Token getTok();

        //----< derived states don't have to know about other states >---
        // When some state's start condition is satisfied, it would return this derived state.

        static public TokenState nextState(TokenContext context)
        {
            int nextItem = context.src.peek(0);
            int nextItem2 = context.src.peek(1);
            int nextItem3 = context.src.peek(2);
            int nextItem4 = context.src.peek(3);

            if (nextItem < 0)
                return null;
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;
            char ch3 = (char)nextItem3;
            char ch4 = (char)nextItem4;

            if (context.ns_.isNewlineStateStart(ch))
                return context.ns_;
            if (context.ws_.isWhiteSpaceStart(ch))
                return context.ws_;
            if (context.as_.isAlphaStateStart(ch))
                return context.as_;
            if (context.ccs_.isCCommentStateStart(ch, ch2))
                return context.ccs_;
            if (context.cppcs_.isCppCommentStateStart(ch, ch2))
                return context.cppcs_;
            if (context.dcsps_.isDoubleCharSpecialPunctStateStart(ch, ch2))
                return context.dcsps_;
            if (context.scsp_.isSingleCharSpecialPunctStateStart(ch))
                return context.scsp_;
            if (context.dqs_.isDoubleQuoteStateStart(ch))
                return context.dqs_;
            if (context.sqs_.isSingleQuoteStateStart(ch, ch2, ch3, ch4))
                return context.sqs_;

            // If all above conditions are not satisfied, it would return PunctState.
            return context.ps_;
        }
        //----< has tokenizer reached the end of its source? >-----------

        public bool isDone()
        {
            if (context_.src == null)
                return true;
            return context_.src.end();
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Derived State Classes
    /* - InitialState                    Initial state and responsible for finding next state.
     * - WhiteSpaceState              Token with space, tab
     * - AlphaNumState                Token with letters, digits and underscore
     * - CCommentState                Token with muti-line comment e.g. /*... */
    /* - CppCommentState              Token with single line comment e.g. //
     * - DoubleCharSpecialPunctState  Token with special double chars e.g. "<<", 
     *                                ">>", "::", "++", "--", "==", "+=", "-=", "*=",
     *                                "/=", "&&", "||", "=>","?:", "()", "[]","?.",
     *                                "&=", "^=", "??", "!=", ">=" and "<=".
     * - SingleCharSpecialPunctState  Token with special single chars e.g. '<', '>',
     *                                '[', ']', '(', ')', '{', '}', ':', '=', '+',
     *                                '-', '*', '/', ';' and ','.    
     * - DoubleQuoteState             Token with double quote marks and the string within them.
     * - SingleQuoteState             Token with single quote marks and the char within them.
     * - NewlineState                 Token with '\n','\r'.
     * - PunctuationState             Token holding anything not included above.
     * ----------------------------------------------------------------
     * - Each state class accepts a reference to the context in its
     *   constructor and saves in its inherited context_ property.
     * - InitialState is the initial state. And Every time when other states
     *   end, it would return to this state. It is responsible for finding
     *   next state.
     * - The getTok() method in InitialState is for finding next state.
     * - The getTok() method in other state is for returning a token 
     *   conforming to its state. 
     * - The getTok() method promises not to extract characters from
     *   the TokenSource that belong to another state.
     * - These requirements lead us to depend heavily on peeking into
     *   the TokenSource's content.
     */

    ///////////////////////////////////////////////////////////////////
    // InitialState - This is the initial state. And Every time when other states
    // end, it would return to this state. 
    // - getTok() in this state is to find the next state and change into it.
    // Author: Yilin Ren

    class InitialState : TokenState
    {
        public InitialState(TokenContext context)
        {
            context_ = context;
        }

        //----< Decide next state and alter current state to it >-------

        override public Token getTok()
        {
            Token tok = new Token();
            TokenState.nextState(context_);
            context_.currentState_ = TokenState.nextState(context_);
            return null;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // WhiteSpaceState class
    // - extracts contiguous whitespace chars as a token
    // - will be thrown away by tokenizer
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    class WhiteSpaceState : TokenState
    {
        public WhiteSpaceState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the start of white space? >-----------------------
        //- WhiteSpaceState starts when next char is space or tab
        public bool isWhiteSpaceStart(int nextItem)
        {
            if (nextItem < 0)              // EOF
                return false;
            char ch = (char)nextItem;
            return Char.IsWhiteSpace(ch) && ch != '\n' && ch != '\r';
        }

        //----< Is it the end of white spaces? >--------------------------
        bool isWhiteSpaceEnd(int nextItem)
        {
            return !isWhiteSpaceStart(nextItem);
        }

        //----< keep extracting until get none-whitespace >---------------
        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());     // first is WhiteSpace

            while (!isWhiteSpaceEnd(context_.src.peek()))  // stop when non-WhiteSpace
            {
                tok.Append((char)context_.src.next());
            }
            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // PunctState class
    // - extracts contiguous punctuation chars as a token
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    class PunctState : TokenState
    {
        public PunctState(TokenContext context)
        {
            context_ = context;
        }

        //----< Is it the end of punctuation? >---------------------------
        //- Punctuation ends when one of the other states' start condition 
        //  is satisfied.
        bool isPunctuationEnd(int nextItem, int nextItem2, int nextItem3, int nextItem4)
        {
            if (nextItem < 0)
            {
                return true;
            }
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;
            char ch3 = (char)nextItem3;
            char ch4 = (char)nextItem4;

            return (context_.ws_.isWhiteSpaceStart(ch)
                    || context_.as_.isAlphaStateStart(ch)
                    || context_.ns_.isNewlineStateStart(ch)
                    || context_.ccs_.isCCommentStateStart(ch, ch2)
                    || context_.cppcs_.isCppCommentStateStart(ch, ch2)
                    || context_.dcsps_.isDoubleCharSpecialPunctStateStart(ch, ch2)
                    || context_.scsp_.isSingleCharSpecialPunctStateStart(ch)
                    || context_.sqs_.isSingleQuoteStateStart(ch, ch2, ch3, ch4)
                    || context_.dqs_.isDoubleQuoteStateStart(ch)
                   );
        }

        //----< keep extracting until get none-punctuator >--------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());       // first is punctuator

            while (!isPunctuationEnd(context_.src.peek(0), context_.src.peek(1),
                                     context_.src.peek(2), context_.src.peek(3)))   // stop when non-punctuator
            {
                tok.Append((char)context_.src.next());
            }
            return tok;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // AlphaState class
    // - extracts contiguous letter, digit or underscore chars as a token
    // - Modified by Yilin Ren. Based on Demo from Dr. Jim Fawcett.

    class AlphaState : TokenState
    {
        public AlphaState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of AlphaState? >-----------------------
        bool isAlphaStateEnd(int nextItem)
        {
            return !isAlphaStateStart(nextItem);
        }

        //------< Is it the start of AlphaState? >-----------------------
        //- AlphaState starts when the next char is letter, digit or underscore.
        public bool isAlphaStateStart(int nextItem)
        {
            if (nextItem < 0)
                return false;
            char ch = (char)nextItem;
            return Char.IsLetterOrDigit(ch) || ch == '_';
        }
        //----< keep extracting until get none-alpha >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());          // first is alpha

            while (!isAlphaStateEnd(context_.src.peek()))    // stop when non-alpha
            {
                tok.Append((char)context_.src.next());
            }
            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // CCommentState class
    // - extracts muti-line comments as a token 
    // - Author: Yilin Ren

    class CCommentState : TokenState
    {
        public CCommentState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of CCommentState? >-----------------------
        //- CComment ends when the next two chars are "*/".
        bool isCCommentStateEnd(int nextItem, int nextItem2)
        {
            if (nextItem < 0 || nextItem2 < 0)
                return true;
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;

            return (ch == '*' && ch2 == '/');
        }

        //------< Is it the start of CCommentState? >-----------------------
        //- CComment starts when the next two chars are "/*".
        public bool isCCommentStateStart(int nextItem, int nextItem2)
        {
            if (nextItem < 0 || nextItem2 < 0)
                return false;
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;

            return (ch == '/' && ch2 == '*');
        }
        //----< keep extracting until get none-ccomment >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());    // first two chars "/*"
            tok.Append((char)context_.src.next());


            while (!isCCommentStateEnd(context_.src.peek(0), context_.src.peek(1)))    // stop when CComment state ends
            {
                tok.Append((char)context_.src.next());
            }


            if (!(context_.src.peek() < 0))             // last two chars "*/"
                tok.Append((char)context_.src.next());
            if (!(context_.src.peek() < 0))
                tok.Append((char)context_.src.next());

            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // CppCommentState class
    // - extracts single-line comment as a token 
    // - Author: Yilin Ren

    class CppCommentState : TokenState
    {
        public CppCommentState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of CppCommentState? >-----------------------
        //- CppComment ends when the next char is '\n'.
        bool isCppCommentStateEnd(int nextItem)
        {
            if (nextItem < 0)
                return true;
            char ch = (char)nextItem;

            return (ch == '\n');
        }

        //------< Is it the end of CppCommentState? >-----------------------
        //- CppComment ends when the next two chars are "//".
        public bool isCppCommentStateStart(int nextItem, int nextItem2)
        {
            if (nextItem < 0 || nextItem2 < 0)
                return false;
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;

            return (ch == '/' && ch2 == '/');
        }

        //----< keep extracting until get none-cppcomment >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());   // first two chars are "//".
            tok.Append((char)context_.src.next());

            while (!isCppCommentStateEnd(context_.src.peek())) // stop when CppComment state ends
            {
                tok.Append((char)context_.src.next());
            }

            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // DoubleCharSpecialPunctState class
    // - extracts Token with special double chars as a token 
    // - e.g. "<<", ">>", "::", "++", "--", "==", "+=", "-=", "*=",
    //        "/=", "&&", "||", "=>","?:", "()", "[]","?.", "&=",
    //        "^=", "??", "!=", ">=" and "<=".
    // - Author: Yilin Ren

    class DoubleCharSpecialPunctState : TokenState
    {
        public DoubleCharSpecialPunctState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the start of DoubleCharSpecialPunctState? >-----------------------
        //- DoubleCharSpecialPunctState starts when the next two chars belong to 
        //  the double-char special punctuation.

        public bool isDoubleCharSpecialPunctStateStart(int nextItem, int nextItem2)
        {
            if (nextItem < 0 || nextItem2 < 0)
                return false;

            string[] DoubleCharSpecialPunctArray =
            {"<<", ">>", "::", "++", "--", "==", "+=", "-=", "*=", "/=", "&&", "||", "=>",
             "&=", "^=", "??", "!=", ">=", "<=", "?:", "()", "[]","?."};

            string next2chars = ((char)nextItem).ToString() + ((char)nextItem2).ToString();

            return (Array.IndexOf(DoubleCharSpecialPunctArray, next2chars) >= 0);
        }

        //----< keep extracting until get none-doublecharspecialpunct >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());    // first two chars are double char special punct.
            tok.Append((char)context_.src.next());

            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // SingleCharSpecialPunctState class
    // - extracts Token with special single char as a token 
    // e.g. '<', '>', '[', ']', '(', ')', '{', '}', ':', '=', '+',
    //      '-', '*', '/', ';' and ','.
    // - Author: Yilin Ren

    class SingleCharSpecialPunctState : TokenState
    {
        public SingleCharSpecialPunctState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the start of SingleCharSpecialPunctState? >-----------------------
        //- SingleCharSpecialPunctState starts when the next char belong to 
        //  the single-char special punctuation.

        public bool isSingleCharSpecialPunctStateStart(int nextItem)
        {
            if (nextItem < 0)
                return false;

            char[] SingleCharSpecialPunctArray =
            {'<', '>', '[', ']', '(', ')', '{', '}', ':', '=', '+', '-', '*', '/', ';', ','};

            return (Array.IndexOf(SingleCharSpecialPunctArray, (char)nextItem) >= 0);
        }

        //----< keep extracting until get none-singlecharspecialpunct >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());   // first char is single-char special punctuation. 

            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // DoubleQuoteState class
    // - extracts double quote marks and the string within them as a Token.
    // - Author: Yilin Ren


    class DoubleQuoteState : TokenState
    {
        int backSlashCount { get; set; } = 0;  //the number of the back slash before the quote.

        public DoubleQuoteState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of DoubleQuoteState? >-----------------------
        //- DoubleQuoteState ends when the next char is double quote and the number of 
        //  contiguous back slashes before the double quote is even.
        bool isDoubleQuoteStateEnd(int prevItem, int nextItem)
        {
            if (nextItem < 0)
                return true;
            char prevch = (char)prevItem;
            char ch = (char)nextItem;

            if ((ch == '"'))
            {
                if (backSlashCount % 2 == 0)  // When the number of \ before " is even, the state ends. 
                {
                    backSlashCount = 0;
                    return true;
                }
                else
                {
                    backSlashCount = 0;
                    return false;
                }
            }

            if (ch == '\\') backSlashCount++;
            else backSlashCount = 0;
            return false;
        }

        //------< Is it the start of DoubleQuoteState? >-----------------------
        //- DoubleQuoteState starts when the next char is double quote.
        public bool isDoubleQuoteStateStart(int nextItem)
        {
            if (nextItem < 0)
                return false;
            char ch = (char)nextItem;
            return (ch == '\"');
        }

        //----< keep extracting until get none-doublequote >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            int prevItem = context_.src.peek(0);
            tok.Append((char)context_.src.next());  // first char is a double quote.


            while (!isDoubleQuoteStateEnd(prevItem, context_.src.peek()))    // stop when double quote state ends
            {
                prevItem = context_.src.peek();
                tok.Append((char)context_.src.next());
            }

            if (!(context_.src.peek() < 0))         // last char is a double quote.
                tok.Append((char)context_.src.next());

            return tok;
        }
    }


    ///////////////////////////////////////////////////////////////////
    // SingleQuoteState class
    // - extracts single quote mark and the char within them as a Token.
    // - Author: Yilin Ren
    class SingleQuoteState : TokenState
    {
        public SingleQuoteState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of SingleQuoteState? >-----------------------
        //- SingleQuoteState ends when the latest three chars do not satisfy 
        //  the syntax of the char with single quotes.
        //- The syntax of the char with single quotes:
        //   a) one char in the single quote. e.g. 'a', ' '.
        //   b) backslash in the single quote.   e.g. '\n','\\','\''.
        bool isSingleQuoteStateEnd(int prevItem2, int prevItem, int nextItem)
        {
            if (nextItem < 0)
                return true;
            char ch = (char)nextItem;
            char prevch = (char)prevItem;
            char prevch2 = (char)prevItem2;

            return ((prevch != '\\' && ch == '\'')
                    || (prevch2 == '\\' && prevch == '\\' && ch == '\''));
        }

        //------< Is it the start of SingleQuoteState? >-----------------------
        //- SingleQuoteState starts when the next four chars satisfies 
        //  the syntax of the char with single quotes.
        public bool isSingleQuoteStateStart(int nextItem, int nextItem2, int nextItem3, int nextItem4)
        {
            if (nextItem < 0 || nextItem2 < 0 || nextItem3 < 0)
                return false;
            char ch = (char)nextItem;
            char ch2 = (char)nextItem2;
            char ch3 = (char)nextItem3;
            char ch4 = (char)nextItem4;
            return ((ch == '\'' && ch2 != '\\' && ch3 == '\'')
                    || (ch == '\'' && ch2 == '\\' && ch4 == '\''));
        }
        //----< keep extracting until get none-singlequotestate >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            int prevItem2 = context_.src.peek(0);
            tok.Append((char)context_.src.next());  // first char is one single quote.
            int prevItem = context_.src.peek(0);
            tok.Append((char)context_.src.next());  // second char is one char in the single quotes.

            while (!isSingleQuoteStateEnd(prevItem2, prevItem, context_.src.peek()))    // stop when single quote state ends
            {
                prevItem2 = prevItem;             //update 
                prevItem = context_.src.peek();   //update
                tok.Append((char)context_.src.next());
            }

            if (!(context_.src.peek() < 0))
                tok.Append((char)context_.src.next()); // last char is another single quote.

            return tok;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // NewlineState class
    // - extracts '\n' and '\r' as a Token.
    // - Author: Yilin Ren

    class NewlineState : TokenState
    {
        public NewlineState(TokenContext context)
        {
            context_ = context;
        }

        //------< Is it the end of NewlineState? >-----------------------
        //- NewlineState ends when the next char is not '\n' or '\r'.
        public bool isNewlineStateEnd(int nextItem)
        {
            return !isNewlineStateStart((char)nextItem);
        }

        //------< Is it the start of NewlineState? >-----------------------
        //- NewlineState starts when the next char is '\n' or '\r'.

        public bool isNewlineStateStart(int nextItem)
        {
            char ch = (char)nextItem;
            return nextItem >= 0 && (ch == '\n' || ch == '\r');
        }

        //----< keep extracting until get none-newline >-------------------

        override public Token getTok()
        {
            Token tok = new Token();
            tok.Append((char)context_.src.next());   // first char is '\n' or '\r'.

            while (!isNewlineStateEnd(context_.src.peek())) // stop when newline state ends
            {
                context_.src.next();
            }

            return tok;
        }
    }

#if (TEST_TOKENIZER)
    public class TestTokenizer
    {
        //-----< Tokenization from souce code file >------------------------
        static bool testToker(string path)
        {
            Toker toker = new Toker();

            string fqf = System.IO.Path.GetFullPath(path);
            if (!toker.open(fqf))
            {
                Console.Write("\n can't open {0}\n", fqf);
                return false;
            }
            else
            {
                Console.Write("\n  * Tokens:");
                Console.Write("\n  =========");
            }
            while (!toker.isDone())
            {
                Token tok = toker.getTok();
                if (tok != null)
                {
                    if (tok.ToString() != "\n" && tok.ToString() != "\r")
                        Console.Write("\n  - line{0, 3} : {1}", toker.lineCount(), tok);
                    else
                        Console.Write("\n  - line{0, 3} : New Line", toker.lineCount());
                }
            }
            Console.Write("\n");
            toker.close();
            return true;
        }

        [STAThread]
        static void Main()
        {
            string refFilePath = "../../Test.txt";
            string testFilePath = System.IO.Path.GetFullPath(refFilePath);
            Console.WriteLine("\n  Processing " + testFilePath);
            testToker(refFilePath);
        }

    }
#endif
}
