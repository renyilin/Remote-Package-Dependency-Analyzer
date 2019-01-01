/////////////////////////////////////////////////////////////////////
// Toker.cs - Collects words from a stream                         //
// ver 1.3                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2018 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This tokenizer is implemented with the State Pattern, and:
 * - Collects words, called tokens, from a stream.  
 * - Discards all whitespace except for newlines which are returned as
 *   single character tokens.
 * - By default, collects and discards all comments, but has an option
 *   to return each comment as a single token.  
 * - Also returns quoted strings and quoted characters as tokens.
 *   Toker correctly handles the C# string @"...".
 * - This package demonstrates how to build a tokenizer based on the 
 *   State Pattern.  
 * - This Instructor's solution meets all requirements of Project #2
 * 
 * Required Files:
 * ---------------
 * Toker.cs
 * 
 * Maintenance History
 * -------------------
 * ver 1.3 : 05 Oct 2018
 * - Changed type tests, e.g., isWhiteSpace(), from static to non-static
 *   and removed the context argument.  Calls to nextState now use the 
 *   currentState_ reference instead of the TokenState class.
 * ver 1.2 : 06 Sep 2018
 * - added NewLineState
 * ver 1.1 : 02 Sep 2018
 * - Changed Toker, TokenState, TokenFileSource, and TokenContext to fix a bug
 *   in setting the initial state.  These changes are cited, below.
 * - Removed TokenState state_ from toker so only TokenContext instance manages 
 *   the current state.
 * - Changed TokenFileSource() to TokenFileSource(TokenContext context) to allow the 
 *   TokenFileSource instance to set the initial state correctly.
 * - Changed TokenState.nextState() to static TokenState.nextState(TokenContext context).
 *   That allows TokenFileSource to use nextState to set the initial state correctly.
 * - Changed TokenState.nextState(context) to treat everything that is not whitespace
 *   and is not a letter or digit as punctuation.  Char.IsPunctuation was not inclusive
 *   enough for Toker.
 * - changed current_ to currentState_ for readability
 * ver 1.0 : 30 Aug 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
  using Token = String;
  using SbToken = StringBuilder;

  ///////////////////////////////////////////////////////////////////
  // ITokenSource interface
  // - Declares operations expected of any source of tokens
  // - Typically we would use either files or strings.  This demo
  //   provides a source only for Files, e.g., TokenFileSource, below.

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
  // ITokenState interface
  // - Declares operations expected of any token gathering state

  public interface ITokenState
  {
    Token getTok();
    bool isDone();
  }

  ///////////////////////////////////////////////////////////////////
  // Toker class
  // - applications need to use only this class to collect tokens

  public class Toker
  {
    private TokenContext context_;   // holds single instance of all states and token source
    public bool doReturnComments { get; set; } = false;
    public string path { get; set; }

    //----< initialize state machine >-------------------------------

    public Toker()
    {
      context_ = new TokenContext();  // context is glue that holds all state machine parts 
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
      return src.open(path);  // if true, src has set initial state
    }
    //----< close source of tokens >---------------------------------

    public void close()
    {
      context_.src.close();
    }
    //----< extract a single token from TokenSource >----------------
    /*
     * Method promises to:
     * - extract all the text for a single token
     * - leave all the text for the next token in the TokenSource
     * - discard all whitespace except for newlines
     * - discard all comments unless doReturnComments is true
     */
    bool overwrite(Token tok)
    {
      if (isWhiteSpace(tok))
        return true;
      if (!doReturnComments && ((isSingleLineComment(tok) || isMultipleLineComment(tok))))
        return true;
      return false;
    }
    //----< extract token >------------------------------------------

    public Token getTok()
    {
      Token tok = null;
      while(!isDone())
      {
        tok = context_.currentState_.getTok();
        context_.currentState_ = context_.currentState_.nextState();
        if (!overwrite(tok))
          break;
      }
      return tok;
    }
    //----< has Toker reached end of its source? >-------------------

    public bool isDone()
    {
      if (context_.currentState_ == null)
        return true;
      return context_.currentState_.isDone();
    }
    //----< return number of newlines encountered in file >----------

    public int lineCount() { return context_.src.lineCount; }

    //----< return set of oneCharTokens >----------------------------

    public HashSet<string> oneCharTokens()
    {
      return context_.currentState_.oneCharTokens_;
    }
    //----< return set of twoCharTokens >----------------------------

    public HashSet<string> twoCharTokens()
    {
      return context_.currentState_.twoCharTokens_;
    }
    //----< add token to special one char tokens >-------------------

    public bool addOneCharToken(string oneCharTok)
    {
      if (oneCharTok.Length > 1)
        return false;
      context_.currentState_.oneCharTokens_.Add(oneCharTok);
      return true;
    }
    //----< remove token from special one char tokens >--------------

    public bool removeOneCharToken(string oneCharTok)
    {
      return context_.currentState_.oneCharTokens_.Remove(oneCharTok);
    }
    //----< add token to special two char tokens >-------------------

    public bool addTwoCharToken(string twoCharTok)
    {
      if (twoCharTok.Length != 2)
        return false;
      context_.currentState_.twoCharTokens_.Add(twoCharTok);
      return true;
    }
    //----< remove token from special two char tokens >--------------

    public bool removeTwoCharToken(string twoCharTok)
    {
      return context_.currentState_.twoCharTokens_.Remove(twoCharTok);
    }
    //----< is this token whitespace? >------------------------------

    public static bool isWhiteSpace(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      return (Char.IsWhiteSpace(tok[0]) && tok[0] != '\n');
    }
    //----< is this token a newline? >-------------------------------

    public static bool isNewLine(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      return (tok[0] == '\n');
    }
    //----< is this token alphanumeric? >----------------------------

    public static bool isAlphaNum(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      return (Char.IsLetterOrDigit(tok[0]) || tok[0] == '_');
    }
    //----< is this token punctuator? >------------------------------

    public static bool isPunctuator(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      //return (!isWhiteSpace(tok) && !isAlphaNum(tok));
      bool test = isWhiteSpace(tok) || isNewLine(tok) || isAlphaNum(tok);
      test = test || isSingleLineComment(tok) || isMultipleLineComment(tok);
      test = test || isSingleQuote(tok) || isDoubleQuote(tok);
      return !test;
    }
    //----< is this token a single line comment? >-------------------

    public static bool isSingleLineComment(Token tok)
    {
      if (tok == null || tok.Length < 2)
        return false;
      if (tok[0] == '/' && tok[1] == '/')
        return true;
      return false;
    }
    //----< is this token a multiple line comment? >-----------------

    public static bool isMultipleLineComment(Token tok)
    {
      if (tok == null || tok.Length < 2)
        return false;
      if (tok[0] == '/' && tok[1] == '*')
        return true;
      return false;
    }
    //----< is this token a double quoted string? >------------------

    public static bool isDoubleQuote(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      char ch = tok[0];
      if (ch == '@')
      {
        char nxt = tok[1];
        return (nxt == '\"');
      }
      return (ch == '\"');
    }
    //----< is this token a single-quoted character? >---------------

    public static bool isSingleQuote(Token tok)
    {
      if (tok == null || tok.Length == 0)
        return false;
      return (tok[0] == '\'');
    }
  }
  ///////////////////////////////////////////////////////////////////
  // TokenContext class
  // - holds all the tokenizer states
  // - holds source of tokens
  // - internal qualification limits access to this assembly

  public class TokenContext
  {
    internal TokenContext()
    {
      wss_ = new WhiteSpaceState(this);
      nls_ = new NewLineState(this);
      pcs_ = new PunctuationState(this);
      ans_ = new AlphaNumState(this);
      slc_ = new SingleLineCommentState(this);
      mlc_ = new MultiLineCommentState(this);
      squ_ = new SingleQuoteState(this);
      dqu_ = new DoubleQuoteState(this);
      currentState_ = wss_;
    }
    internal WhiteSpaceState wss_ { get; set; }
    internal NewLineState nls_ { get; set; }
    internal PunctuationState pcs_ { get; set; }
    internal AlphaNumState ans_ { get; set; }
    internal SingleLineCommentState slc_ { get; set; }
    internal MultiLineCommentState mlc_ { get; set; }
    internal SingleQuoteState squ_ { get; set; }
    internal DoubleQuoteState dqu_ { get; set; }

    internal TokenState currentState_ { get; set; }
    internal ITokenSource src { get; set; }  // can hold any derived class
  }

  ///////////////////////////////////////////////////////////////////
  // TokenState class
  // - base for all the tokenizer states

  public abstract class TokenState : ITokenState
  {
    internal HashSet<string> oneCharTokens_ { get; set; }
    internal HashSet<string> twoCharTokens_ { get; set; }
    internal TokenContext context_ { get; set; }  // derived classes store context ref here

    public TokenState()
    {
      oneCharTokens_ = new HashSet<string>
      {
        "<", ">", "[", "]", "(", ")", "{", "}", ".", ";", "=", "+", "-", "*"
      };
      twoCharTokens_ = new HashSet<string>
      {
        "<<", ">>", "::", "++", "--", "==", "+=", "-=", "*=", "/=", "&&", "||"
      };
    }
    //----< return set of oneCharTokens >----------------------------

    public HashSet<string> oneCharTokens()
    {
      return oneCharTokens_;
    }
    //----< return set of twoCharTokens >----------------------------

    public HashSet<string> twoCharTokens()
    {
      return twoCharTokens_;
    }
    //----< add token to special one char tokens >-------------------

    public bool addOneCharToken(string oneCharTok)
    {
      if (oneCharTok.Length > 1)
        return false;
      oneCharTokens_.Add(oneCharTok);
      return true;
    }
    //----< remove token from special one char tokens >--------------

    public bool removeOneCharToken(string oneCharTok)
    {
      return oneCharTokens_.Remove(oneCharTok);
    }
    //----< add token to special two char tokens >-------------------

    public bool addTwoCharToken(string twoCharTok)
    {
      if (twoCharTok.Length != 2)
        return false;
      twoCharTokens_.Add(twoCharTok);
      return true;
    }
    //----< remove token from special two char tokens >--------------

    public bool removeTwoCharToken(string twoCharTok)
    {
      return twoCharTokens_.Remove(twoCharTok);
    }
    //----< delegate source opening to context's src >---------------

    public bool open(string path)
    {
      return context_.src.open(path);
    }
    //----< leave implementation to derived states >-----------------

    public abstract Token getTok();

    //----< what's next in the TokenSource? >------------------------

    public bool isWhiteSpace()
    {
      char ch = (char)context_.src.peek();
      return (Char.IsWhiteSpace(ch) && ch != '\n');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isNewLine()
    {
      return ((char)context_.src.peek() == '\n');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isAlphaNum()
    {
      char ch = (char)context_.src.peek();
      return (Char.IsLetterOrDigit(ch) || ch == '_');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isSingleLineComment()
    {
      int first = context_.src.peek();
      int second = context_.src.peek(1);
      char chFirst = (char)first;
      char chSecond = (char)second;
      return (chFirst == '/' && chSecond == '/');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isMultiLineComment()
    {
      int first = context_.src.peek();
      int second = context_.src.peek(1);
      char chFirst = (char)first;
      char chSecond = (char)second;
      return (chFirst == '/' && chSecond == '*');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isDoubleQuote()
    {
      char ch = (char)context_.src.peek();
      if(ch == '@')
      {
        char nxt = (char)context_.src.peek(1);
        return (nxt == '\"');
      }
      return (ch == '\"');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isSingleQuote()
    {
      char ch = (char)context_.src.peek();
      return (ch == '\'');
    }
    //----< what's next in the TokenSource? >------------------------

    public bool isPunctuation()
    {
      bool test = isWhiteSpace() || isNewLine() || isAlphaNum();
      test = test || isSingleLineComment() || isMultiLineComment();
      test = test || isSingleQuote() || isDoubleQuote();
      return !test;
    }
    //----< return next state based on content of TokenSource >------

    public TokenState nextState()
    {
      int first = context_.src.peek();
      if (first < 0)
        return null;

      if (isWhiteSpace())
        return context_.wss_;

      if (isNewLine())
        return context_.nls_;

      if (isAlphaNum())
        return context_.ans_;

      if (isSingleLineComment())
        return context_.slc_;

      if(isMultiLineComment())
        return context_.mlc_;

      if(isDoubleQuote())
        return context_.dqu_;

      if(isSingleQuote())
        return context_.squ_;

      // toker's definition of punctuation is anything that is not:
      // - whitespace     space, tab, return
      // - newline
      // - alphanumeric   abc123
      // - comment        /* comment */ or // comment
      // - quote          'a' or "a string"

      // Char.IsPunctuation is not inclusive enough

      return context_.pcs_;
    }
    //----< has tokenizer reached the end of its source? >-----------

    public bool isDone() {
      if (context_.src == null)
        return true;
      return context_.src.end();
    }
    //----< helper function to handle escaped chars in states >------
    /*
     * Tests to see if last char in token is preceded by an odd number
     * of escape chars, e.g.:
     * \\\' is escaped
     * \\"  is not escaped
     */
    protected bool isEscaped(Token tok)
    {
      int size = tok.Length;
      if (size < 2)
        return false;
      int count = 0;
      for (int i = 0; i < size - 1; ++i)
      {
        count = i % 2;
        if (tok[size - i - 2] != '\\')
          break;
      }
      if (count == 0)
        return false;
      return true;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Derived State Classes      getTok() returns
  // -------------------------  -------------------------------------
  /* - WhiteSpaceState          Token with space, tab, and return chars
   * - NewLineState             Token with newline
   * - AlphaNumState            Token with letters, digits, and underscore
   * - SingleLineCommentState   Token holding C++ style comment
   * - MultiLineCommentState    Token holding C style comment
   * - SingleQuoteState         Token holding a quoted character
   * - DoubleQuoteState         Token holding a quoted string
   * - PunctuationState         Token holding anything not included above
   * ----------------------------------------------------------------
   * - Each state class accepts a reference to the context in its
   *   constructor and saves in its inherited context_ property.
   * - It is only required to provide a getTok() method which
   *   returns a token conforming to its state, e.g., whitespace, ...
   * - getTok() assumes that the TokenSource's first character 
   *   matches its type e.g., whitespace char, ...
   * - The nextState() method ensures that the condition, above, is
   *   satisfied.
   * - The getTok() method promises not to extract characters from
   *   the TokenSource that belong to another state.
   * - These requirements lead us to depend heavily on peeking into
   *   the TokenSource's content.
   */
  ///////////////////////////////////////////////////////////////////
  // WhiteSpaceState class
  // - extracts, from context_.src, contiguous whitespace chars as token
  // - will be thrown away by tokenizer

  public class WhiteSpaceState : TokenState
  {
    public WhiteSpaceState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get non-whitespace >---------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());     // first is WhiteSpace

      while (context_.currentState_.isWhiteSpace())  // stop when non-WhiteSpace
      {
        tok.Append((char)context_.src.next());
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // NewLineState class
  // - extracts, from context_.src, a single newline character

  public class NewLineState : TokenState
  {
    public NewLineState(TokenContext context)
    {
      context_ = context;
    }
    //----< return first char in src, as it must be a newline >------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());     // first is newline

      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // AlphaNumState class
  // - extracts contiguous letter and digit chars as a token

  public class AlphaNumState : TokenState
  {
    public AlphaNumState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get non-alphanum >-----------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());  // first is alphanum

      while (isAlphaNum())            // stop when non-alphanum
      {
        tok.Append((char)context_.src.next());
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // SingleLineCommentState class
  // - extracts single line comment as a token

  public class SingleLineCommentState : TokenState
  {
    public SingleLineCommentState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get newline >--------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());   // char is /
      tok.Append((char)context_.src.next());   // char is /

      char ch;
      while (true)   // stop when newline
      {
        ch = (char)context_.src.peek();
        if (ch == '\n')
          break;
        tok.Append((char)context_.src.next());
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // MulitpleLineComment class
  // - extracts multiple line comment as a token

  public class MultiLineCommentState : TokenState
  {
    public MultiLineCommentState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get comment termintor >------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());       // char is /
      tok.Append((char)context_.src.next());       // char is *

      char ch = ' ', prevCh = ' ';
      while (true)   // stop when newline
      {
        prevCh = ch;
        ch = (char)context_.src.next();
        tok.Append(ch);
        if (prevCh == '*' && ch == '/')
          break;
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // SingleQuoteState class
  // - extracts single quoted char as a token with quotes

  public class SingleQuoteState : TokenState
  {
    public SingleQuoteState(TokenContext context)
    {
      context_ = context;
    }
    
    //----< keep extracting until get end quote >--------------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());       // char is '\''

      while (true) 
      {
        char ch = (char)context_.src.next();
        tok.Append(ch);
        if (ch == '\'' && !isEscaped(tok.ToString()))
          break;
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // DoubleQuoteState class
  // - extracts text in quotes as a token

  public class DoubleQuoteState : TokenState
  {
    public DoubleQuoteState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get end quote >--------------------

    override public Token getTok()
    {
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());       // char is "\"" or "@"
      char nxt = (char)context_.src.peek();
      if (nxt == '\"' && tok[0] == '@')
        tok.Append((char)context_.src.next());

      while (true)
      {
        char ch = (char)context_.src.next();
        tok.Append(ch);
        if (ch == '\"' && (!isEscaped(tok.ToString()) || tok[0] == '@'))
          break;
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // PunctuationState class
  // - extracts contiguous punctuation chars as a token

  public class PunctuationState : TokenState
  {
    public PunctuationState(TokenContext context)
    {
      context_ = context;
    }
    //----< keep extracting until get non-punctuator >---------------
    /*
     * Here is where we handle single char and two char special tokens
     * as well as other punctuators.
     */
    override public Token getTok()
    {
      // is this a two char special token?
      SbToken test = new SbToken();
      test.Append((char)context_.src.peek());
      test.Append((char)context_.src.peek(1));
      if(twoCharTokens_.Contains(test.ToString()))
      {
        context_.src.next();  // pop peeked char
        context_.src.next();  // pop peeked char
        return test.ToString();
      }
      // is this a single char special token?
      SbToken tok = new SbToken();
      tok.Append((char)context_.src.next());       // pop first punctuator
      if (oneCharTokens_.Contains(tok.ToString()))
        return tok.ToString();

      // not special token, so continue collecting punctuation chars
      while(context_.currentState_.isPunctuation())
      {
        // check for other special cases starting in middle of punctuator
        if (
          isMultiLineComment() || isSingleLineComment() ||
          isDoubleQuote() || isSingleQuote()
        )
          break;
        tok.Append((char)context_.src.next());
      }
      return tok.ToString();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // TokenSourceFile class
  // - extracts integers from token source
  // - Streams often use terminators that can't be represented by
  //   a character, so we collect all elements as ints
  // - keeps track of the line number where a token is found
  // - uses StreamReader which correctly handles byte order mark
  //   characters and alternate text encodings.

  public class TokenSourceFile : ITokenSource
  {
    public int lineCount { get; set; } = 1;
    public string path { get; set; }
    private System.IO.StreamReader fs_;           // physical source of text
    private List<int> charQ_ = new List<int>();   // enqueing ints but using as chars
    private TokenContext context_;

    public TokenSourceFile(TokenContext context)
    {
      context_ = context;
    }
    //----< attempt to open file with a System.IO.StreamReader >-----

    public bool open(string path)
    {
      this.path = path;
      try
      {
        fs_ = new System.IO.StreamReader(path,true);
        context_.currentState_ = context_.currentState_.nextState();
      }
      catch(Exception ex)
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
        charQ_.RemoveAt(0);   // pop from queue
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
     */
    public int peek(int n=0)
    {
      if(n < charQ_.Count)  // nth already peeked, so return it
      {
        return charQ_[n];
      }
      else                  // nth int not yet peeked
      {
        for(int i=charQ_.Count; i<=n; ++i)
        {
          if (end()) 
            return -1;
          charQ_.Add(fs_.Read());  // read and enqueue
        }
        return charQ_[n];   // now return the last peeked
      }
    }
    //----< reached the end of the file stream? >--------------------

    public bool end()
    {
      return fs_.EndOfStream;
    }
  }

#if(TEST_TOKER)

  class DemoToker
  {
    //----< tokenize a test file >-----------------------------------
    /*
     * This method allows us to easily test against several different
     * files with special cases that have to be handled correctly.
     */
    static bool testTokerWithComments(string path)
    {
      Toker toker = new Toker();
      toker.doReturnComments = true;

      string fqf = System.IO.Path.GetFullPath(path);
      if (!toker.open(fqf))
      {
        Console.Write("\n can't open {0}\n", fqf);
        return false;
      }
      else
      {
        Console.Write("\n  processing file: {0}\n", fqf);
      }
      while (!toker.isDone())
      {
        Token tok = toker.getTok();
        if (Toker.isNewLine(tok))
        {
          tok = "newline";
        }
        else if (Toker.isMultipleLineComment(tok))  // this is a cosmetic
          tok = "\n" + tok;
        Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
      }
      toker.close();
      Console.Write("\n");
      return true;
    }
    static bool testTokerWithoutComments(string path)
    {
      Toker toker = new Toker();
      toker.doReturnComments = false;

      string fqf = System.IO.Path.GetFullPath(path);
      if (!toker.open(fqf))
      {
        Console.Write("\n can't open {0}\n", fqf);
        return false;
      }
      else
      {
        Console.Write("\n  processing file: {0}\n", fqf);
      }
      while (!toker.isDone())
      {
        Token tok = toker.getTok();
        if (Toker.isNewLine(tok))
        {
          tok = "newline";
        }
        else if (Toker.isMultipleLineComment(tok))  // this is a cosmetic
          tok = "\n" + tok;
        Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
      }
      toker.close();
      Console.Write("\n");
      return true;
    }
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrate Toker class");
      Console.Write("\n =========================");

      StringBuilder msg = new StringBuilder();
      msg.Append("\n  Some things this Instructor's Solution does for CSE681 Project #2:");
      msg.Append("\n  - collect comments as tokens");
      msg.Append("\n  - collect double quoted strings as tokens");
      msg.Append("\n  - collect single quoted strings as tokens");
      msg.Append("\n  - collect specified single characters as tokens");
      msg.Append("\n  - collect specified character pairs as tokens");
      msg.Append("\n  - integrate with a SemiExpression collector");
      msg.Append("\n  - provide the required package structure");
      msg.Append("\n");

      Console.Write(msg);

      testTokerWithComments("../../Test.txt");
      testTokerWithoutComments("../../Toker.cs");

      Console.Write("\n\n");
    }
  }
}
  
#endif
