using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Parser
    {
        /// <summary>
        /// Parses a number token to some specific number, that is
        /// integer, double etc.
        /// </summary>
        /// <param name="token">A number token.</param>
        /// <returns>Internal representation for a number.</returns>
        private static INumber NumberParser (NumberToken token)
        {
            System.Numerics.BigInteger intOut = 0;
            if (Utils.ParseInteger (token.AsString, out intOut))
                return new ScmIntegerNumber (intOut);

            double dblOut = 0;
            if (Utils.ParseDouble (token.AsString, out dblOut))
                return new ScmDoubleNumber (dblOut);

            throw new ParserException (UserMessages.MalformedNumber, token.Line, token.Column, token.Id);
        }

        /// <summary>
        /// Checks whether the given program contains a balanced number of
        /// paranthesis. Throws an exception on failure, returns the same
        /// list on success.
        /// </summary>
        /// <param name="tokens">Tokens consisting a program.</param>
        /// <returns>Input list on success.</returns>
        private static List<Token> CheckParanthesisBalance (List<Token> tokens)
        {
            // Saves each opening paranthesis on the stack, and pops
            // parantheses from the stack when we get a closing paranthesis.
            // If parantheses do not match - throws an exception.
            // We're using the stack to keep track on which paranthesis exactly
            // didn't have a matching paranthesis.
            var stack = new Stack<Token> ();
            foreach (var token in tokens)
            {
                if (token is BeginToken)
                {
                    stack.Push (token);
                }
                else if (token is EndToken)
                {
                    bool match = (stack.Count > 0 && stack.Peek () is BeginToken);
                    if (!match)
                    {
                        stack.Push (token);
                        break;
                    }
                    stack.Pop ();
                }
            }
            if (stack.Count > 0)
            {
                Token token = stack.Peek ();
                throw new ParserException (UserMessages.UnbalancedParanthesis, token.Line, token.Column, token.Id);
            }
            return tokens;
        }

        /// <summary>
        /// This method adds expr to the currExpr, if the latter is not null, and to the program
        /// otherwise.
        /// </summary>
        /// <param name="expr">Expression to add.</param>
        /// <param name="currExpr">Current enclosing expression.</param>
        /// <param name="program">Program.</param>
        private static void AddExpression (IExpression expr, ScmCombination currExpr, List<IExpression> program)
        {
            if (currExpr != null)
            {
                currExpr.Expressions.Add (expr);
                return;
            }
            program.Add (expr);
        }

        /// <summary>
        /// This methods analyses the given tokens and produces a list of combinations
        /// and expressions that can be parsed further for syntax consistency etc.
        /// </summary>
        /// <param name="tokens">A list of tokens.</param>
        /// <returns>A list of combinations and expressions.</returns>
        private static List<IExpression> CombineExpressions (List<Token> tokens)
        {
            CheckParanthesisBalance (tokens);

            var program = new List<IExpression> ();
            var eStack = new Stack<IExpression> ();
            ScmCombination currentCombination = null;

            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];

                if (token is BeginToken)
                {
                    if (currentCombination != null)
                    {
                        eStack.Push (currentCombination);
                    }
                    currentCombination = new ScmCombination (token);
                }
                else if (token is EndToken)
                {
                    IExpression cExpr = currentCombination as IExpression;
                    currentCombination = cExpr as ScmCombination;
                    if (eStack.Count > 0)
                    {
                        var stackExprOnTop = eStack.Peek () as ScmCombination;
                        stackExprOnTop.Expressions.Add (currentCombination);
                        currentCombination = eStack.Pop () as ScmCombination;
                    }
                    else
                    {
                        program.Add (currentCombination);
                        currentCombination = null;
                    }
                }
                else
                {
                    if (token is SymbolToken)
                    {
                        IExpression scmSymbol = new ScmSymbol (token);
                        AddExpression (scmSymbol, currentCombination, program);
                    }
                    else if (token is NumberToken)
                    {
                        IExpression scmNumber = NumberParser (token as NumberToken);
                        AddExpression (scmNumber, currentCombination, program);
                    }
                    else if (token is StringToken)
                    {
                        IExpression scmString = new ScmString (token);
                        AddExpression (scmString, currentCombination, program);
                    }
                    else if (token is DotToken)
                    {
                        IExpression scmString = new ScmDotModifier (token);
                        AddExpression (scmString, currentCombination, program);
                    }
                    else if (token is QuoteToken || token is QuasiquoteToken || token is UnquoteToken)
                    {
                        IExpression scmQuote = null;
                        if (token is QuoteToken)
                            scmQuote = new ScmQuoteSpecialForm (null);
                        else if (token is UnquoteToken)
                            scmQuote = new ScmUnquoteSpecialForm (null);
                        else
                            scmQuote = new ScmQuasiquoteSpecialForm (null);
                        AddExpression (scmQuote, currentCombination, program);
                    }
                }
            }

            return QuotationSyntacticSugar (program, false);
        }

        /// <summary>
        /// Parses a list of tokens and produces a runnable program. May throw
        /// an exception.
        /// </summary>
        /// <param name="tokens">A list of tokens.</param>
        /// <returns>A runnable program.</returns>
        public static List<IExpression> Parse (List<Token> tokens)
        {
            // Combine expressions and check syntax for consistency.
            return Parse (CombineExpressions (tokens));
        }
    }
}
