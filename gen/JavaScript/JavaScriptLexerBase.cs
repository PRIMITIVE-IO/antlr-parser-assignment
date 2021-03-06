using Antlr4.Runtime;
using System.Collections.Generic;

/// <summary>
/// All lexer methods that used in grammar (IsStrictMode)
/// should start with Upper Case Char similar to Lexer rules.
/// </summary>
public abstract class JavaScriptLexerBase : Lexer
{
    /// <summary>
    /// Stores values of nested modes. By default mode is strict or
    /// defined externally(useStrictDefault)
    /// </summary>
    private Stack<bool> scopeStrictModes = new Stack<bool>();

    private IToken _lastToken = null;

    /// <summary>
    /// Default value of strict mode
    /// Can be defined externally by changing UseStrictDefault
    /// </summary>
    private bool _useStrictDefault = false;

    /// <summary>
    /// Current value of strict mode
    /// Can be defined during parsing, see StringFunctions.js and StringGlobal.js samples
    /// </summary>
    private bool _useStrictCurrent = false;

    public JavaScriptLexerBase(ICharStream input)
        : base(input)
    {
    }

    public bool IsStartOfFile(){
        return _lastToken == null;
    }

    public bool UseStrictDefault
    {
        get
        {
            return _useStrictDefault;
        }
        set
        {
            _useStrictDefault = value;
            _useStrictCurrent = value;
        }
    }

    public bool IsStrictMode()
    {
        return _useStrictCurrent;
    }

    /// <summary>
    /// Return the next token from the character stream and records this last
    /// token in case it resides on the default channel. This recorded token
    /// is used to determine when the lexer could possibly match a regex
    /// literal.
    /// 
    /// </summary>
    /// <returns>
    /// The next token from the character stream.
    /// </returns>
    public override IToken NextToken()
    {
        // Get the next token.
        IToken next = base.NextToken();

        if (next.Channel == DefaultTokenChannel)
        {
            // Keep track of the last token on the default channel.
            _lastToken = next;
        }

        return next;
    }

    protected void ProcessOpenBrace()
    {
        _useStrictCurrent = scopeStrictModes.Count > 0 && scopeStrictModes.Peek() ? true : UseStrictDefault;
        scopeStrictModes.Push(_useStrictCurrent);
    }

    protected void ProcessCloseBrace()
    {
        _useStrictCurrent = scopeStrictModes.Count > 0 ? scopeStrictModes.Pop() : UseStrictDefault;
    }

    protected void ProcessStringLiteral()
    {
        if (_lastToken == null || _lastToken.Type == 9) // open brace
        {
            if (Text.Equals("\"use strict\"") || Text.Equals("'use strict'"))
            {
                if (scopeStrictModes.Count > 0)
                    scopeStrictModes.Pop();
                _useStrictCurrent = true;
                scopeStrictModes.Push(_useStrictCurrent);
            }
        }
    }

    /// <summary>
    /// Returns true if the lexer can match a regex literal.
    /// </summary>
    protected bool IsRegexPossible()
    {
        if (_lastToken == null)
        {
            // No token has been produced yet: at the start of the input,
            // no division is possible, so a regex literal _is_ possible.
            return true;
        }

        switch (_lastToken.Type)
        {
            // THESE WERE MANUALLY ASSIGNED BY JOHN
            case 117: //Identifier:
            case 59: //NullLiteral:
            case 60: //BooleanLiteral:
            case 88: //This:
            case 6: //CloseBracket:
            case 8: //CloseParen:
            case 63: //OctalIntegerLiteral:
            case 61: //DecimalLiteral:
            case 62: //HexIntegerLiteral:
            case 118: //StringLiteral:
            case 18://PlusPlus:
            case 19: //MinusMinus:
                // After any of the tokens above, no regex literal can follow.
                return false;
            default:
                // In all other cases, a regex literal _is_ possible.
                return true;
        }
    }
}
