using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    partial class Parser
    {
        // TODO: Add special forms:
        //  let* ?
        //  fluid-let ?
        //  case
        //  iteration let, do ?

        delegate IExpression SpecialFormParser (IExpression expression);
        static Dictionary<string, SpecialFormParser> m_SFParsers = new Dictionary<string, SpecialFormParser> ();

        static Parser ()
        {
            m_SFParsers.Add ("if", IfSpecialFormParser);
            m_SFParsers.Add ("let", LetSpecialFormParser);
            m_SFParsers.Add ("letrec", LetrecSpecialFormParser);
            m_SFParsers.Add ("lambda", LambdaSpecialFormParser);
            m_SFParsers.Add ("define", DefineSpecialFormParser);
            m_SFParsers.Add ("set!", SetSpecialFormParser);
            m_SFParsers.Add ("cond", CondSpecialFormParser);
            m_SFParsers.Add ("quote", QuoteSpecialFormParser);
            m_SFParsers.Add ("quasiquote", QuasiquoteSpecialFormParser);
            m_SFParsers.Add ("unquote", UnquoteSpecialFormParser);
            m_SFParsers.Add ("begin", BeginSpecialFormParser);
            m_SFParsers.Add ("and", AndOrSpecialFormParser);
            m_SFParsers.Add ("or", AndOrSpecialFormParser);
        }

        private static IExpression AndOrSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            var expressionList = new List<IExpression> ();
            for (int i = 0; i < combination.Operands.Count; ++i)
            {
                expressionList.Add (ParseExpression (combination.Operands[i]));
            }
            var proc = combination.Procedure as ScmSymbol;
            if (proc.Token.AsString.Equals("and"))
                return new ScmAndSpecialForm (expressionList);
            return new ScmOrSpecialForm (expressionList);
        }

        private static IExpression BeginSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            if (!combination.HasOperands)
                throw new ParserException (UserMessages.MustContainAtLeastOneExpression, combination);

            var expressionList = new List<IExpression> ();
            for (int i = 0; i < combination.Operands.Count; ++i)
            {
                expressionList.Add (ParseExpression (combination.Operands[i]));
            }
            return new ScmBeginSpecialForm (expressionList);
        }

        private static IExpression QuotationSpecialFormParser<T> (IExpression expression, Func<IExpression, T> creator)
            where T : IExpression
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
                throw new ParserException (UserMessages.MustContainOneExpressionAsArgument, combination);
            var datum = ParseExpression (combination.Operands.First ());
            if (datum is ScmCombination)
            {
                var datumAsCombination = datum as ScmCombination;
                if (datumAsCombination.Expressions.IsEmpty ())
                    datum = ScmEmptyList.Instance;
                else
                    datum = new ScmPair (datumAsCombination.Expressions);
            }
            var quoteSpecialForm = creator (datum);
            return quoteSpecialForm;
        }

        private static IExpression UnquoteSpecialFormParser (IExpression expression)
        {
            return QuotationSpecialFormParser (expression, x => new ScmUnquoteSpecialForm (x));
        }

        private static IExpression QuasiquoteSpecialFormParser (IExpression expression)
        {
            return QuotationSpecialFormParser (expression, x => new ScmQuasiquoteSpecialForm (x));
        }

        private static IExpression QuoteSpecialFormParser (IExpression expression)
        {
            return QuotationSpecialFormParser (expression, x => new ScmQuoteSpecialForm (x));
        }

        private static IExpression CondSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            // DrRacket doesn't require clauses, but MIT/Scheme does. I had choosen to 
            // require at least one clause.
            if (combination.Operands.IsEmpty ())
                throw new ParserException (UserMessages.MustContainAtLeastOneClause, combination);

            var condSpecialForm = new ScmCondSpecialForm ();
            foreach (var clauseCombination in combination.Operands)
            {
                var clause = clauseCombination as ScmCombination;
                if (clause == null)
                    throw new ParserException (UserMessages.ExpectingOneClause, clauseCombination);

                for (int i = 0; i < clause.Expressions.Count; ++i)
                    clause.Expressions[i] = ParseExpression (clause.Expressions[i]);

                condSpecialForm.AddClause (clause);
            }

            return condSpecialForm;
        }

        private static IExpression IfSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            IExpression predicate = null;
            IExpression consequent = null;
            IExpression alternative = ScmUnassigned.Instance;

            var exprCombLen = combination.Expressions.Count;
            if (exprCombLen < 3 || exprCombLen > 4)
            {
                throw new ParserException (UserMessages.MalformedIf, combination);
            }

            predicate = ParseExpression (combination.Expressions[1]);
            consequent = ParseExpression (combination.Expressions[2]);
            if (exprCombLen == 4)
                alternative = ParseExpression (combination.Expressions[3]);

            return new ScmIfSpecialForm (predicate, consequent, alternative);
        }

        private static IExpression LetSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            var bindingsMap = new Dictionary<IExpression, IExpression> ();

            var exprCombLen = combination.Expressions.Count;
            if (exprCombLen < 3)
            {
                throw new ParserException (UserMessages.MalformedLet, combination);
            }

            var bindings = combination.Expressions[1] as ScmCombination;
            if (bindings == null)
                throw new ParserException (UserMessages.ExpectingBindingList, bindings);

            if (bindings.Expressions == null || bindings.Expressions.Count == 0)
                throw new ParserException (UserMessages.EmptyBindingList, bindings);

            for (int i = 0; i < bindings.Expressions.Count; ++i)
            {
                var bindingPair = bindings.Expressions[i] as ScmCombination;
                if (bindingPair == null || bindingPair != null && bindingPair.Expressions.Count != 2)
                    throw new ParserException (UserMessages.ExpectingPair, bindings);
                var bindingName = bindingPair.Expressions.Head () as ScmSymbol;
                if (bindingName == null)
                    throw new ParserException (UserMessages.ExpectingName, bindings);
                var bindingExpression = bindingPair.Expressions[1];
                bindingsMap.Add (bindingName, ParseExpression (bindingExpression));
            }

            var expressionList = new List<IExpression> ();
            for (int i = 2; i < combination.Expressions.Count; ++i)
            {
                var parsedExpression = ParseExpression (combination.Expressions[i]);
                if (parsedExpression is ScmCombination)
                {
                    var parsedExprComn = parsedExpression as ScmCombination;
                    if (parsedExprComn.IsEmpty)
                    {
                        throw new ParserException (UserMessages.IllegalExpression, parsedExprComn);
                    }
                }
                expressionList.Add (parsedExpression);
            }

            return new ScmLetSpecialForm (bindingsMap, expressionList);
        }

        private static bool FormalParametersParser (List<IExpression> formals)
        {
            if (formals.Count >= 2)
            {
                var lastFormal = formals.Last ();
                var oneBeforeLastFormal = formals.OneBeforeLast ();
                if (oneBeforeLastFormal is ScmDotModifier)
                {
                    if (!(lastFormal is ScmSymbol))
                    {
                        throw new ParserException ("Illegal use of \".\"", oneBeforeLastFormal);
                    }
                    formals.Remove (oneBeforeLastFormal);
                    return true;
                }
            }
            return false;
        }

        private static IExpression LambdaSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            var exprCombLen = combination.Expressions.Count;
            if (exprCombLen < 3)
            {
                throw new ParserException (UserMessages.MalformedLambda, combination);
            }

            var formalParams = combination.Expressions[1] as ScmCombination;
            if (formalParams == null)
            {
                throw new ParserException (UserMessages.LambdaMustHaveParamList, combination);
            }

            var dottedArgumentList = FormalParametersParser (formalParams.Expressions);
            var formals = new List<ScmSymbol> ();
            for (int i = 0; i < formalParams.Expressions.Count; ++i)
            {
                var paramName = formalParams.Expressions[i] as ScmSymbol;
                if (paramName == null)
                {
                    throw new ParserException (UserMessages.MustBeIdentifier, formalParams);
                }
                formals.Add (paramName);
            }

            var expressions = new List<IExpression> ();
            for (int i = 2; i < combination.Expressions.Count; ++i)
            {
                var parsedExpression = ParseExpression (combination.Expressions[i]);
                if (parsedExpression is ScmCombination)
                {
                    var parsedExprComn = (ScmCombination)parsedExpression;
                    if (parsedExprComn.IsEmpty)
                    {
                        throw new ParserException (UserMessages.IllegalExpression, parsedExprComn);
                    }
                }
                expressions.Add (parsedExpression);
            }

            return new ScmLambdaSpecialForm (formals, expressions, dottedArgumentList);
        }

        private static IExpression LetrecSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            var bindingsMap = new Dictionary<IExpression, IExpression> ();

            var exprCombLen = combination.Expressions.Count;
            if (exprCombLen < 3)
            {
                throw new ParserException (UserMessages.MalformedLetrec, combination);
            }

            var bindings = combination.Expressions[1] as ScmCombination;
            if (bindings == null)
                throw new ParserException (UserMessages.ExpectingBindingList, bindings);

            if (bindings.Expressions == null || bindings.Expressions.Count == 0)
                throw new ParserException (UserMessages.EmptyBindingList, bindings);

            for (int i = 0; i < bindings.Expressions.Count; ++i)
            {
                var bindingPair = bindings.Expressions[i] as ScmCombination;
                if (bindingPair == null || bindingPair != null && bindingPair.Expressions.Count != 2)
                    throw new ParserException (UserMessages.ExpectingPair, bindingPair);
                var bindingName = bindingPair.Expressions.Head () as ScmSymbol;
                if (bindingName == null)
                    throw new ParserException (UserMessages.ExpectingName, bindingName);
                var bindingExpr = bindingPair.Expressions[1];
                bindingsMap.Add (bindingName, ParseExpression (bindingExpr));
            }

            var expressions = new List<IExpression> ();
            for (int i = 2; i < combination.Expressions.Count; ++i)
            {
                expressions.Add (ParseExpression (combination.Expressions[i]));
            }

            return new ScmLetrecSpecialForm (bindingsMap, expressions);
        }

        private static IExpression DefineSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            if (combination.Expressions.Count < 2)
                throw new ParserException (UserMessages.MalformedDefine, combination);

            if (combination.Expressions[1] is ScmSymbol)
            {
                var symName = combination.Expressions[1] as ScmSymbol;
                IExpression defValue = ScmUnassigned.Instance;
                if (combination.Expressions.Count == 3)
                {
                    defValue = ParseExpression (combination.Expressions[2]);
                }
                return new ScmDefineVarSpecialForm (symName, defValue);
            }

            var nameAndFormals = (combination.Expressions[1] as ScmCombination);
            if (nameAndFormals == null || nameAndFormals != null && nameAndFormals.Expressions.Count == 0)
                throw new ParserException (UserMessages.MustHaveNameFormals, combination);

            var procName = nameAndFormals.Expressions.Head () as ScmSymbol;
            if (procName == null)
                throw new ParserException (UserMessages.MustBeIdentifier, combination);

            var formalParams = nameAndFormals.Expressions.Tail ();
            var dottedArgumentList = FormalParametersParser (formalParams);

            var formalsAsSymbols = new List<ScmSymbol> ();
            for (int i = 0; i < formalParams.Count; ++i)
            {
                var formalParameter = formalParams[i];
                var formalParameterAsSym = formalParams[i] as ScmSymbol;
                if (formalParameterAsSym == null)
                    throw new ParserException (UserMessages.MustBeIdentifier, nameAndFormals);
                formalsAsSymbols.Add (formalParameterAsSym);
            }

            var expressions = combination.Expressions.TailFrom (2);
            if (expressions.Count == 0)
                throw new ParserException (UserMessages.MustHaveBody, combination);

            for (int i = 0; i < expressions.Count; ++i)
            {
                expressions[i] = ParseExpression (expressions[i]);
            }

            return new ScmDefineProcSpecialForm (procName, formalsAsSymbols, expressions, dottedArgumentList);
        }

        private static IExpression SetSpecialFormParser (IExpression expression)
        {
            var combination = expression as ScmCombination;
            if (combination.Expressions.Count < 2)
                throw new ParserException (UserMessages.MalformedSet, combination);

            var symName = combination.Expressions[1] as ScmSymbol;
            if (symName == null)
                throw new ParserException (UserMessages.MustBeIdentifier, combination);

            IExpression varValue = ScmUnassigned.Instance;
            if (combination.Expressions.Count == 3)
            {
                varValue = ParseExpression (combination.Expressions[2]);
            }

            return new ScmSetSpecialForm (symName, varValue);
        }

        private static bool IsSpecialForm (IExpression expression)
        {
            var combination = expression as ScmCombination;
            if (combination == null)
                return false;

            var exprCombLen = combination.Expressions.Count;
            if (exprCombLen == 0)
                return false;

            var opSym = combination.Expressions.Head () as ScmSymbol;
            if (opSym == null)
                return false;

            return m_SFParsers.ContainsKey (opSym.Token.AsString);
        }

        private static IExpression ParseSpecialForm (IExpression expression)
        {
            var combination = expression as ScmCombination;
            var operatorSymbol = combination.Expressions.Head () as ScmSymbol;
            return m_SFParsers[operatorSymbol.Token.AsString] (combination);
        }

        private static IExpression ParseCombination (IExpression expression)
        {
            var combination = expression as ScmCombination;
            if (IsSpecialForm (combination))
            {
                return ParseSpecialForm (combination);
            }
            for (int i = 0; i < combination.Expressions.Count; ++i)
            {
                combination.Expressions[i] = ParseExpression (combination.Expressions[i]);
            }
            return expression;
        }

        private static IExpression ParseExpression (IExpression expression)
        {
            if (expression is ScmCombination)
            {
                return ParseCombination ((ScmCombination)expression);
            }
            return expression;
        }

        private static List<IExpression> Parse (List<IExpression> program)
        {
            for (int i = 0; i < program.Count; ++i)
            {
                program[i] = ParseExpression (program[i]);
            }
            return program;
        }
    }
}
