using System;
using System.Collections.Generic;
using Antlr4.Runtime;

/// <summary>
/// All parser methods that used in grammar (p, prev, notLineTerminator, etc.)
/// should start with lower case char similar to parser rules.
/// </summary>
public abstract class JavaScriptParserBase : Parser
{
    private readonly Stack<string> _tagNames = new Stack<string>();
    public JavaScriptParserBase(ITokenStream input)
        : base(input)
    {
    }

    /// <summary>
    /// Short form for prev(String str)
    /// </summary>
    protected bool p(string str)
    {
        return prev(str);
    }

    /// <summary>
    /// Whether the previous token value equals to str
    /// </summary>
    protected bool prev(string str)
    {
        return TokenStream.LT(-1).Text.Equals(str);
    }

    // Short form for next(String str)
    protected bool n(string str)
    {
        return next(str);
    }

    // Whether the next token value equals to @param str
    protected bool next(string str)
    {
        return TokenStream.LT(1).Text.Equals(str);
    }

    protected bool notLineTerminator()
    {
        return !here(121);//LineTerminator);
    }

    protected bool notOpenBraceAndNotFunction()
    {
        int nextTokenType = TokenStream.LT(1).Type;
        return nextTokenType != 9 /*OpenBrace*/ && nextTokenType != 87; // Function;
    }

    protected bool closeBrace()
    {
        return TokenStream.LT(1).Type == 10; //CloseBrace;
    }

    /// <summary>Returns true if on the current index of the parser's
    /// token stream a token of the given type exists on the
    /// Hidden channel.
    /// </summary>
    /// <param name="type">
    /// The type of the token on the Hidden channel to check.
    /// </param>
    protected bool here(int type)
    {
        // Get the token ahead of the current index.
        int possibleIndexEosToken = CurrentToken.TokenIndex - 1;
        IToken ahead = TokenStream.Get(possibleIndexEosToken);

        // Check if the token resides on the Hidden channel and if it's of the
        // provided type.
        return ahead.Channel == Lexer.Hidden && ahead.Type == type;
    }

    /// <summary>
    /// Returns true if on the current index of the parser's
    /// token stream a token exists on the Hidden channel which
    /// either is a line terminator, or is a multi line comment that
    /// contains a line terminator.
    /// </summary>
    protected bool lineTerminatorAhead()
    {
        // Get the token ahead of the current index.
        int possibleIndexEosToken = CurrentToken.TokenIndex - 1;
        IToken ahead = TokenStream.Get(possibleIndexEosToken);

        if (ahead.Channel != Lexer.Hidden)
        {
            // We're only interested in tokens on the Hidden channel.
            return false;
        }

        if (ahead.Type == 121) //  LineTerminator)
        {
            // There is definitely a line terminator ahead.
            return true;
        }

        if (ahead.Type == 120) //WhiteSpaces)
        {
            // Get the token ahead of the current whitespaces.
            possibleIndexEosToken = CurrentToken.TokenIndex - 2;
            ahead = TokenStream.Get(possibleIndexEosToken);
        }

        // Get the token's text and type.
        string text = ahead.Text;
        int type = ahead.Type;

        // Check if the token is, or contains a line terminator.
        return (type == 2 /* MultiLineComment*/ && (text.Contains("\r") || text.Contains("\n"))) ||
               (type == 121);//LineTerminator);
    }

    protected void pushHtmlTagName(string tagName)
    {
        _tagNames.Push(tagName);
    }

    protected bool popHtmlTagName(string tagName)
    {
        return string.Equals(_tagNames.Pop(),tagName, StringComparison.InvariantCulture);
    }
}
