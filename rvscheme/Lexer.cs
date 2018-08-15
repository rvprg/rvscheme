using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace rvscheme
{
    /// <summary>
    /// This class is used to store string data and meta data, such as line and column
    /// numbers, and an id, that identifies where this string comes from.
    /// </summary>
    class Token
    {
        int m_Line = 1;
        int m_Column = 1;
        string m_Id = String.Empty;

        public string Id
        {
            get { return m_Id; }
        }

        public int Column
        {
            get { return this.m_Column; }
        }

        public int Line
        {
            get { return this.m_Line; }
        }

        protected string m_String;
        public string AsString
        {
            get { return m_String; }
        }

        public Token (string v, int l, int c, string id)
        {
            m_String = v;
            m_Line = l;
            m_Column = c;
            m_Id = id;
        }

        public Token (string v)
        {
            m_String = v;
            m_Line = -1;
            m_Column = -1;
        }

        public Token ()
        {
            m_String = String.Empty;
            m_Line = -1;
            m_Column = -1;
        }
    }

    /// <summary>
    /// A token that denotes the beginning of a block.
    /// </summary>
    sealed class BeginToken : Token
    {
        public BeginToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A token that denotes the end of a block.
    /// </summary>
    sealed class EndToken : Token
    {
        public EndToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A number token (a double, an integer, a rational etc.)
    /// </summary>
    sealed class NumberToken : Token
    {
        public NumberToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    ///  A quoted string token.
    /// </summary>
    sealed class StringToken : Token
    {
        public StringToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A symbol (identifier) token.
    /// </summary>
    sealed class SymbolToken : Token
    {
        public SymbolToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    abstract class QuotationToken : Token
    {
        public QuotationToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A quosiquote (syntax sugar `) token.
    /// </summary>
    sealed class QuasiquoteToken : QuotationToken
    {
        public QuasiquoteToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A quote (syntax sugar ') token.
    /// </summary>
    sealed class QuoteToken : QuotationToken
    {
        public QuoteToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// An unquote (syntax sugar ,) token.
    /// </summary>
    sealed class UnquoteToken : QuotationToken
    {
        public UnquoteToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// A dot token. Acts as a modifier when placed before
    /// the last argument in a procedure's list of formal
    /// arguments, denoting that the procedure accepts variable
    /// number of arguments, which will be grouped in a list and
    /// will be bound to the dotted argument.
    /// </summary>
    sealed class DotToken : Token
    {
        public DotToken (string v, int l, int c, string id)
            : base (v, l, c, id)
        {
        }
    }

    /// <summary>
    /// Reads a stream of chars and produces a list of tokens.
    /// </summary>
    sealed class Lexer
    {
        const char LeftParanthesis = '(';
        const char RightParanthesis = ')';
        const char Comment = ';';
        const char EndOfStream = '\0';
        const char EndOfLine = '\n';
        const char DoubleQuote = '"';

        int m_Column = 1;
        int m_Line = 1;
        string m_Id = String.Empty;

        TextReader m_Source = null;
        String m_Token = String.Empty;

        private int ReadChar ()
        {
            int c = m_Source.Read ();
            if (IsEndOfLine ((char)c))
            {
                m_Column = 1;
                m_Line++;
            }
            else
            {
                m_Column++;
            }
            return c;
        }

        private char SkipWhiteSpaces ()
        {
            while (IsWhiteSpace (NextCharacter ()))
                ReadChar ();
            return NextCharacter ();
        }

        private char SkipComment ()
        {
            while (IsEndOfComment (NextCharacter ()))
                ReadChar ();
            ReadChar ();
            return NextCharacter ();
        }

        private bool IsWhiteSpace (char c)
        {
            return char.IsWhiteSpace (c);
        }

        private bool IsEndOfStream (char c)
        {
            return c == EndOfStream;
        }

        private bool IsEndOfLine (char c)
        {
            return c == EndOfLine;
        }

        private bool IsDigit (char c)
        {
            return char.IsDigit (c) || c.Equals ('+') || c.Equals ('-') || (c.Equals ('.') && char.IsDigit (NextCharacter ()));
        }

        private bool IsDoubleQuote (char c)
        {
            return c == DoubleQuote;
        }

        private bool IsEndOfComment (char c)
        {
            return IsEndOfStream (c) || IsEndOfLine (c);
        }

        private bool IsLeftParanthesis (char c)
        {
            return c == LeftParanthesis;
        }

        private bool IsRightParanthesis (char c)
        {
            return c == RightParanthesis;
        }

        private bool IsComment (char c)
        {
            return c == Comment;
        }

        private void AppendCharToToken ()
        {
            int c = ReadChar ();
            if (c == -1)
                throw new Exception (UserMessages.UnexpectedEndOfStream);
            m_Token += (char)c;
        }

        private char NextCharacter ()
        {
            int c = m_Source.Peek ();
            if (c == -1)
                return EndOfStream;
            return (char)c;
        }

        private bool IsDot (char s)
        {
            return s.Equals ('.');
        }

        private bool IsUnquote (char s)
        {
            return s.Equals (',');
        }

        private bool IsQuasiquote (char s)
        {
            return s.Equals ('`');
        }

        private bool IsQuote (char s)
        {
            return s.Equals ('\'');
        }

        private bool IsDelimiter (char s)
        {
            return IsWhiteSpace (s) || IsLeftParanthesis (s) || IsRightParanthesis (s) || s.Equals (';') || s.Equals ('"') || s.Equals ('\'') || s.Equals ('|') || s.Equals (',') || s.Equals ('`');
        }

        private bool IsEndOfToken (char s)
        {
            return IsDelimiter (s) || IsEndOfStream (s);
        }

        private Token ReadNumberToken ()
        {
            while (!IsEndOfToken (NextCharacter ()))
                AppendCharToToken ();
            bool isNumber = Utils.IsNumber (m_Token);
            if (!isNumber)
            {
                return new SymbolToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
            }
            return new NumberToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadBeginToken ()
        {
            AppendCharToToken ();
            return new BeginToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadEndToken ()
        {
            AppendCharToToken ();
            return new EndToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadStringToken ()
        {
            AppendCharToToken ();
            while (!IsDoubleQuote (NextCharacter ()))
                AppendCharToToken ();
            AppendCharToToken ();
            return new StringToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadSymbolToken ()
        {
            while (!IsEndOfToken (NextCharacter ()))
                AppendCharToToken ();
            return new SymbolToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadQuasiquoteToken ()
        {
            AppendCharToToken ();
            return new QuasiquoteToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadQuoteToken ()
        {
            AppendCharToToken ();
            return new QuoteToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadUnquoteToken ()
        {
            AppendCharToToken ();
            return new UnquoteToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        private Token ReadDotToken ()
        {
            AppendCharToToken ();
            while (IsDot (NextCharacter ()) && !IsEndOfToken (NextCharacter ()))
                AppendCharToToken ();
            if (m_Token.Length !=1 && m_Token.Length != 3)
                throw new ParserException ("Illegal sequence of dots", m_Line, m_Column - m_Token.Length, m_Id);
            return new DotToken (m_Token, m_Line, m_Column - m_Token.Length, m_Id);
        }

        public Token ReadToken ()
        {
            if (m_Source == null)
                throw new Exception (UserMessages.SourceIsNotSet);

            m_Token = String.Empty;
            char nextChar = NextCharacter ();
            if (IsEndOfStream (nextChar))
                return null;
            if (IsWhiteSpace (nextChar))
                nextChar = SkipWhiteSpaces ();
            if (IsComment (nextChar))
                nextChar = SkipComment ();

            if (IsLeftParanthesis (nextChar))
            {
                return ReadBeginToken ();
            }
            else if (IsRightParanthesis (nextChar))
            {
                return ReadEndToken ();
            }
            else if (IsDoubleQuote (nextChar))
            {
                return ReadStringToken ();
            }
            else if (IsDigit (nextChar))
            {
                return ReadNumberToken ();
            }
            else if (IsQuote (nextChar))
            {
                return ReadQuoteToken ();
            }
            else if (IsUnquote (nextChar))
            {
                return ReadUnquoteToken ();
            }
            else if (IsQuasiquote (nextChar))
            {
                return ReadQuasiquoteToken ();
            }
            else if (IsDot (nextChar))
            {
                return ReadDotToken ();
            }
            else if (!IsEndOfStream (nextChar))
            {
                return ReadSymbolToken ();
            }

            return null;
        }

        public List<Token> GetTokens (string source, string sourceId)
        {
            return GetTokens (new StringReader (source), sourceId);
        }

        public List<Token> GetTokens (TextReader source, string sourceId)
        {
            m_Column = 1;
            m_Line = 1;
            m_Source = source;
            m_Id = sourceId;
            var tokens = new List<Token> ();
            Token token = ReadToken ();
            while (token != null)
            {
                tokens.Add (token);
                token = ReadToken ();
            }
            return tokens;
        }
    }
}

