// ------------------------------------------------------------------
//
// Copyright (c) 2013 Bruno Vier Hoffmeister <bruno at hoffmeister.us>.
// BSB Licence (See the file License.txt for the license details.)
// All rights reserved.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace NumericRegularExpressions
{
    /*
    <Main> : <Commands> 
           | <StartAnchor> <Commands> 

    <StartAnchor>: '^' 
    <EndAnchor>    : '$' 
   
   <Commands>: <Number> ' ' <Commands> 
                | <Integer> ' ' <Commands> 
                | <Interval> <Commands> 
                | <Number> <Limiter> <Commands> 
                | <Integer>  <Limiter>  <Commands> 
                | '(' <Commands> ')' <Commands> 
                | '(' <Commands> ')' <Limiter> <Commands> 
                | <Verifier>  <Commands>
                | <Verifier>  <Limiter>  <Commands>
                | <EndAnchor> <EOF> 
                | <EOF> 

    <Interval>: '[' <IntervaNumber> ':' <IntervaNumber> ']' 
                | '[' <IntervaNumber> ':' <IntervaNumber> ']'  <Limiter>
                | '[' <IntervaNumber> ':' <IntervaNumber> ';' <Function> ']' 
                | '[' <IntervaNumber> ':' <IntervaNumber> ';' <Function> ']'  <Limiter> 
 
    <IntervaNumber> : <Number> 
                    | <Integer> 
                    | <Percent> 
     
    <Limiter>: <quantifiers> 
                | ‘{’ <NumericLimiter> ‘}’
                | ‘{’ <PercentageLimiter> ‘}’
                | ‘{’ <PercentageLimiter> ';' <NumericLimiter>  ‘}’

    <quantifiers>       : '*' 
                        | '+' 
                        | '?' 

   <Verifier>  : ‘=’ <IntervaNumber>
                | ‘>’ <IntervaNumber>
	| ‘>=’ <IntervaNumber>
	| ‘<’ <IntervaNumber>
	| ‘<=’ <IntervaNumber>

    <NumericLimiter>: <Integer> 
                            | <Integer> ':' <Integer> 

    <PercentageLimiter>: <Percent> 
                        | <Percent> ':' <Percent> 

    <Function>: <FunctionName> 
                : <FunctionName> <Parameters> 

    <Parameters> : <Number>  
                 | <Integer> 
                 | <Number> <Parameters> 
                 | <Integer> <Parameters> 

   <FunctionName> : [a-zA-Z][a-zA-Z0-9_]* 
   
   <Integer>    : [0-9]+ 
                 | -[0-9]+ 

    <Number>     : [0-9]+[,.][0-9]+ 
                 | -[0-9]+[,.][0-9]+ 
   
   <Percent>    : <Number> '%' 
                 | <Integer> '%' 

    */
    class CompiledNumericRegex<T> where T : IConvertible, IComparable<T>
    {
        public List<CompiledBlockBase> Blocks{get; private set;}
        public List<CallbackFunction<T>> Functions { get; private set; }
        public bool StartAnchor { get; private set; }
        public bool EndAnchor { get; private set; }

        private string Pattern;
        private int CurrentPosition;
        private char LookAhead;
        private string Lexeme;
        private StringBuilder LexemeStream;
        private CompiledBlocksTypes Token;
        private Stack<int> SemanticStack;

        public CompiledNumericRegex(string pattern)
        {
            StartAnchor = EndAnchor = false;
            Compile(pattern);
        }

        private List<CompiledBlockBase> Compile(string pattern)
        {
            Pattern = pattern;
            CurrentPosition = -1;
            Blocks = new List<CompiledBlockBase>();
            Functions = new List<CallbackFunction<T>>();
            SemanticStack = new Stack<int>();
            LexemeStream = new StringBuilder();

            //Positioning look ahed at the first char. Get the first token
            MoveLookAHead();
            GetNextToken();

            SyntaticAnalyses();

            return Blocks;
        }

        #region Syntatic
        private void SyntaticAnalyses()
        {
            Main();    
        }

        private void Main()
        {
            if (Token == CompiledBlocksTypes.StartAnchor)
            {
                StartAnchor = true;
                GetNextToken();
            }

            Commands();

            if (SemanticStack.Count > 0)
            {
                throw new NumericRegexSyntaxException("Unclosed Parentheses", CurrentPosition);
            }

            LexemeStream = null;
            Lexeme = null;
            SemanticStack = null;
        }

        private void Commands()
        {
            if (Token == CompiledBlocksTypes.Number || Token == CompiledBlocksTypes.Integer)
            {
                Blocks.Add(new NumericCompiledBlock<T>(Lexeme));
                GetNextToken();

                Limiter(Blocks.Count - 1);

                Commands();
            }
            else if (Token == CompiledBlocksTypes.OpenInterval)
            {
                Interval();
                Limiter(Blocks.Count - 1);
                Commands();
            }
            else if (Token == CompiledBlocksTypes.OpenParentheses)
            {
                SemanticStack.Push(Blocks.Count);
                Blocks.Add(new TokenCompiledBlock<T>(Token));
                GetNextToken();
                Commands();
            }
            else if (Token == CompiledBlocksTypes.CloseParentheses)
            {
                int value = SemanticStack.Pop();
                Blocks.Add(new TokenCompiledBlock<T>(Token));
                GetNextToken();

                Limiter(value);

                Commands();
            }
            else if (Token == CompiledBlocksTypes.GreaterThan || Token == CompiledBlocksTypes.LessThan || Token == CompiledBlocksTypes.Equals)
            {
                CheckerType type = ((Token == CompiledBlocksTypes.Equals) ? CheckerType.Equals : ((Token == CompiledBlocksTypes.GreaterThan) ? CheckerType.GreaterThan : CheckerType.LessThan));
                GetNextToken();
                if (Token == CompiledBlocksTypes.Equals && type != CheckerType.Equals)
                {
                    type = ((type == CheckerType.GreaterThan) ? CheckerType.EqualsOrGreaterThan : CheckerType.EqualsOrLessThan);
                    GetNextToken();
                }

                if (Token != CompiledBlocksTypes.Number && Token != CompiledBlocksTypes.Integer && Token != CompiledBlocksTypes.Percent)
                {
                    throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Number/Percent", Token.ToString()), CurrentPosition);
                }

                Blocks.Add(new CheckerCompiledBlock<T>(type, Lexeme, Token == CompiledBlocksTypes.Percent));
                GetNextToken();

                Limiter(Blocks.Count - 1);
                Commands();
            }
            else if (Token == CompiledBlocksTypes.EndAnchor)
            {
                EndAnchor = true;
            }
            else if (Token == CompiledBlocksTypes.EOF)
            {
                return;
            }
            else
            {
                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}", Token.ToString()), CurrentPosition);
            }
        }

        private void Interval()
        {
            string StartNumber = null, StartPercent = null, EndNumber = null, EndPercent = null, Function = null;
            GetNextToken();

            if (Token == CompiledBlocksTypes.Number || Token == CompiledBlocksTypes.Integer)
            {
                StartNumber = Lexeme;
            }
            else if (Token == CompiledBlocksTypes.Percent)
            {
                StartPercent = Lexeme;
            }
            else
            {
                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Number/Percent", Token.ToString()), CurrentPosition);
            }
            GetNextToken();

            if (Token != CompiledBlocksTypes.SeparatorColon)
            {
                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected :", Token.ToString()), CurrentPosition);
            }
            GetNextToken();

            if (Token == CompiledBlocksTypes.Number || Token == CompiledBlocksTypes.Integer)
            {
                EndNumber = Lexeme;
            }
            else if (Token == CompiledBlocksTypes.Percent)
            {
                EndPercent = Lexeme;
            }
            else
            {
                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Number/Percent", Token.ToString()), CurrentPosition);
            }
            GetNextToken();

            List<T> Parameters = new List<T>();
            if (Token == CompiledBlocksTypes.SeparatorSemiColon)
            {
                GetNextToken();
                if (Token != CompiledBlocksTypes.Function)
                {
                    throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Function", Token.ToString()), CurrentPosition);
                }
                Function = Lexeme;
                GetNextToken();

                while (Token == CompiledBlocksTypes.Integer || Token == CompiledBlocksTypes.Number)
                {
                    Parameters.Add((T)Convert.ChangeType(Lexeme, typeof(T)));
                    GetNextToken();
                }
            }

            if (Token != CompiledBlocksTypes.CloseInterval)
            {
                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected ]", Token.ToString()), CurrentPosition);
            }

            if (!string.IsNullOrEmpty(Function))
            {
                Functions.Add(new CallbackFunction<T>(Function, null, Parameters.Count));
            }

            Blocks.Add(new IntervalCompiledBlock<T>(StartNumber, EndNumber, StartPercent, EndPercent, Function, Parameters.ToArray()));

            GetNextToken();
        }

        private void Limiter(int Position)
        {
            if (Token == CompiledBlocksTypes.Asterisk || Token == CompiledBlocksTypes.Plus || Token == CompiledBlocksTypes.Question)
            {
                List<CompiledBlockBase> repetitionBlock = Blocks.GetRange(Position, Blocks.Count - Position);
                Blocks.RemoveRange(Position, Blocks.Count - Position);

                if (Token == CompiledBlocksTypes.Asterisk)
                {
                    Blocks.Add(new LimiterCompiledBlock<T>(repetitionBlock));
                }
                else if (Token == CompiledBlocksTypes.Plus)
                {
                    Blocks.Add(new LimiterCompiledBlock<T>(repetitionBlock, null, "1"));
                }
                else if (Token == CompiledBlocksTypes.Question)
                {
                    Blocks.Add(new LimiterCompiledBlock<T>(repetitionBlock, "1", "0"));
                 }
                GetNextToken();
            }
            else if (Token == CompiledBlocksTypes.OpenLimiter)
            {
                GetNextToken();
                if (Token == CompiledBlocksTypes.Integer)
                {
                    string EndNumber = null;
                    string StartNumber = Lexeme;
                    GetNextToken();
                    if (Token == CompiledBlocksTypes.SeparatorColon)
                    {
                        GetNextToken();
                        if (Token != CompiledBlocksTypes.Integer)
                        {
                            throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Integer", Token.ToString()), CurrentPosition);
                        }
                        EndNumber = Lexeme;
                        GetNextToken();
                    }
                    if (Token != CompiledBlocksTypes.CloseLimiter)
                    {
                        throw new NumericRegexSyntaxException(string.Format(@"Unexpected token {0}. Expected {1}", Token.ToString(), "}"), CurrentPosition);
                    }
                    GetNextToken();
                    Blocks.Add(new LimiterCompiledBlock<T>(Blocks.GetRange(Position, Blocks.Count - Position), EndNumber, StartNumber));
                    Blocks.RemoveRange(Position, Blocks.Count - Position -1);
                }
                else if (Token == CompiledBlocksTypes.Percent)
                {
                    string StartPercentage = Lexeme;
                    string EndPercentage = null;
                    string StartNumber = null;
                    string EndNumber = null;

                    GetNextToken();
                    if (Token == CompiledBlocksTypes.SeparatorColon)
                    {
                        GetNextToken();
                        if (Token != CompiledBlocksTypes.Percent)
                        {
                            throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Percent", Token.ToString()), CurrentPosition);
                        }
                        EndPercentage = Lexeme;
                        GetNextToken();
                    }
                    if (Token == CompiledBlocksTypes.SeparatorSemiColon)
                    {
                        GetNextToken();
                        if (Token != CompiledBlocksTypes.Integer)
                        {
                            throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Integer", Token.ToString()), CurrentPosition);
                        }
                        StartNumber = Lexeme;
                        GetNextToken();
                        if (Token == CompiledBlocksTypes.SeparatorColon)
                        {
                            GetNextToken();
                            if (Token != CompiledBlocksTypes.Integer)
                            {
                                throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected Integer", Token.ToString()), CurrentPosition);
                            }
                            EndNumber = Lexeme;
                            GetNextToken();
                        }
                    }
                    if (Token != CompiledBlocksTypes.CloseLimiter)
                    {
                        throw new NumericRegexSyntaxException(string.Format(@"Unexpected token {0}. Expected {1}", Token.ToString(), "}"), CurrentPosition);
                    }
                    GetNextToken();
                    Blocks.Add(new LimiterCompiledBlock<T>(Blocks.GetRange(Position, Blocks.Count - Position), EndNumber, StartNumber, EndPercentage, StartPercentage));
                    Blocks.RemoveRange(Position, Blocks.Count - Position - 1);
                }
                else
                {
                    throw new NumericRegexSyntaxException(string.Format("Unexpected token {0}. Expected {1}", Token.ToString(), "Integer/Percent"), CurrentPosition);
                }
            }
        }

        #endregion

        #region Lexical
        private void GetNextToken()
        {
            while (LookAhead == ' ')
            {
                MoveLookAHead();
            }

            if ((LookAhead >= '0' && LookAhead <= '9') || LookAhead == '-')
            {
                LexemeStream.Clear();
                LexemeStream.Append(LookAhead);
                MoveLookAHead();

                bool FoundPoint = false;
                while ((LookAhead >= '0' && LookAhead <= '9') || LookAhead == '.' || LookAhead == ',')
                {
                    if (LookAhead == '.' || LookAhead == ',')
                    {
                        if (FoundPoint)
                        {
                            throw new NumericRegexLexicalException(string.Format("Invalid number at {0} ({1})", Pattern, CurrentPosition), CurrentPosition);
                        }
                        FoundPoint = true;
                        LexemeStream.Append('.');
                    }
                    else
                    {
                        LexemeStream.Append(LookAhead);
                    }
                    MoveLookAHead();
                }

                if (LookAhead == '%')
                {
                    Token = CompiledBlocksTypes.Percent;
                    MoveLookAHead();
                }
                else if (FoundPoint)
                {
                    Token = CompiledBlocksTypes.Number;
                }
                else
                {
                    Token = CompiledBlocksTypes.Integer;
                }
                Lexeme = LexemeStream.ToString();
            }
            else if ((LookAhead >= 'a' && LookAhead <= 'z') || (LookAhead >= 'A' && LookAhead <= 'Z'))
            {
                LexemeStream.Clear();
                LexemeStream.Append(LookAhead);
                MoveLookAHead();
                while ((LookAhead >= 'a' && LookAhead <= 'z')|| (LookAhead >= 'A' && LookAhead <= 'Z') || (LookAhead >= '0' && LookAhead < '9'))
                {
                    LexemeStream.Append(LookAhead);
                    MoveLookAHead();
                }
                Token = CompiledBlocksTypes.Function;
                Lexeme = LexemeStream.ToString();
            }
            else
            {
                switch (LookAhead)
                {
                    case '[':
                        Token = CompiledBlocksTypes.OpenInterval;
                        break;
                    case ']':
                        Token = CompiledBlocksTypes.CloseInterval;
                        break;
                    case '{':
                        Token = CompiledBlocksTypes.OpenLimiter;
                        break;
                    case '}':
                        Token = CompiledBlocksTypes.CloseLimiter;
                        break;
                    case ':':
                        Token = CompiledBlocksTypes.SeparatorColon;
                        break;
                    case ';':
                        Token = CompiledBlocksTypes.SeparatorSemiColon;
                        break;
                    case '*':
                        Token = CompiledBlocksTypes.Asterisk;
                        break;
                    case '+':
                        Token = CompiledBlocksTypes.Plus;
                        break;
                    case '?':
                        Token = CompiledBlocksTypes.Question;
                        break;
                    case '(':
                        Token = CompiledBlocksTypes.OpenParentheses;
                        break;
                    case ')':
                        Token = CompiledBlocksTypes.CloseParentheses;
                        break;
                    case '^':
                        Token = CompiledBlocksTypes.StartAnchor;
                        break;
                    case '$':
                        Token = CompiledBlocksTypes.EndAnchor;
                        break;
                    case '>':
                        Token = CompiledBlocksTypes.GreaterThan;
                        break;
                    case '<':
                        Token = CompiledBlocksTypes.LessThan;
                        break;
                    case '=':
                        Token = CompiledBlocksTypes.Equals;
                        break;
                    case '\0':
                        Token = CompiledBlocksTypes.EOF;
                        break;
                    default:
                        throw new NumericRegexLexicalException(string.Format("Unexpected character {0}", LookAhead), CurrentPosition);
                }

                MoveLookAHead();
            }
        }

        private void MoveLookAHead()
        {
            if (CurrentPosition + 1 >= Pattern.Length)
            { 
                LookAhead = '\0';
                return;
            }
            LookAhead = Pattern[++CurrentPosition];
        }
        #endregion

    }
}
