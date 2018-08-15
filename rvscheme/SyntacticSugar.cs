using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    /// <summary>
    /// These methods process syntactic sugar for quotations, that is quote ('), unquote (,) and
    /// quasiquote (`).
    /// </summary>
    sealed partial class Parser
    {
        /// <summary>
        /// This method takes a list of expressions. The list should have dummy quotation objects
        /// (such as ScmQuoteSpecialForm) with the Datum field being null. We will look for all the
        /// dummy quotations and process expressions that follow them. For instance, if we
        /// have a list [Quote(null), Number], then the result of the transformation will be the
        /// list [Quote(Number)].
        /// </summary>
        /// <param name="program">A list of expressions.</param>
        /// <param name="isQuoted">Should be true when called recursively from another quoted expression.</param>
        /// <returns>Processed list of expression ready for evaluation.</returns>
        private static List<IExpression> QuotationSyntacticSugar (List<IExpression> program, bool isQuoted)
        {
            // Iterate over the list of expressions.
            for (int i = 0; i < program.Count; ++i)
            {
                var currExpression = program[i];
                // If the expression we are inspecting is a combination, then
                // process it separately.
                if (currExpression is ScmCombination)
                {
                    // Processed combination may have quoted expressions as well, so this same method
                    // will be called on the list of expressions the combination has. Depending on
                    // the isQuoted flag, a combination or a list will be returned.
                    program[i] = QuotationSyntacticSugar ((ScmCombination)currExpression, isQuoted);
                }
                else if (currExpression is AbstractQuotation)
                {
                    // If the expression is a quotation, however, we will need to find an
                    // expression it quotes.
                    int j = program.FindIndex (i, x => !(x is AbstractQuotation));
                    // If there's nothing to quote we singal an error.
                    if (j == -1)
                        throw new ParserException ("Error", currExpression);
                    // Since there may be a sequence of quotes, we get that sequence and
                    // process the quotes; after that we substitute the dummy quote with a real one.
                    program[i] = QuoteExpression (program.GetRange (i, j - i), program[j]);
                    // Remove the dummy quotes from the program.
                    program.RemoveRange (i + 1, j - i);
                }
            }
            return program;
        }

        /// <summary>
        /// Quotes an expression. If the expression is a combination, then each expression within
        /// the combination will be processed (i.e. quoted if necessary) and the resulting expressions
        /// will be returned as a combination or a list, depending on the isQuoted flag. The
        /// flag should be set to true, if the combination we are processing is within some other
        /// quoted combination. If the expression is not combination, then the expression is
        /// not processed and returned unmodified.
        /// </summary>
        /// <param name="expression">An expression to process.</param>
        /// <param name="isQuoted">True if this combination is within quoted combination.</param>
        /// <returns>Expression itself, a list or a combination.</returns>
        private static IExpression QuotationSyntacticSugar (IExpression expression, bool isQuoted)
        {
            if (!(expression is ScmCombination))
                return expression;
            var scmCombination = (ScmCombination)expression;
            var expressions = QuotationSyntacticSugar (scmCombination.Expressions, isQuoted);
            if (isQuoted)
                return new ScmPair (expressions);
            return new ScmCombination (expressions);
        }

        /// <summary>
        /// Takes a sequence of quotes and an expression to quote and returns quoted expression.
        /// </summary>
        /// <param name="quoteList">A list of quotes.</param>
        /// <param name="expression">An expression to quote.</param>
        /// <returns>Quoted expression.</returns>
        private static IExpression QuoteExpression (List<IExpression> quoteList, IExpression expression)
        {
            AbstractQuotation quotedExpression = null;
            var quoteListCount = quoteList.Count;
            for (int i = 0; i < quoteListCount; ++i)
            {
                // We process them in an inside-out manner.
                var quote = quoteList[quoteListCount - i - 1];
                // isQuoted is true here, since the expression could be a combination, and we want
                // any combination within that combination to be converted to lists when we quote
                // it.
                if (quote is ScmQuoteSpecialForm)
                    quotedExpression = new ScmQuoteSpecialForm (QuotationSyntacticSugar (expression, true));
                else if (quote is ScmUnquoteSpecialForm)
                    quotedExpression = new ScmUnquoteSpecialForm (QuotationSyntacticSugar (expression, true));
                else if (quote is ScmQuasiquoteSpecialForm)
                    quotedExpression = new ScmQuasiquoteSpecialForm (QuotationSyntacticSugar (expression, true));
                expression = quotedExpression;
            }
            return quotedExpression;
        }
    }
}
