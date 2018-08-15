using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// The expressions are evaluated sequentially from left to right, and the value of the last
        /// expression is returned.
        /// </summary>
        private Bounce ApplyBeginSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var beginSpecialForm = expression as ScmBeginSpecialForm;
            return EvalList (beginSpecialForm.Expressions.AllButLast (), env, evalResults =>
            {
                return Eval (beginSpecialForm.Expressions.Last (), env, cont);
            });
        }

        private delegate Bounce UnquoteHandler (ScmPair pair, List<IExpression> res, Environment env, Cont cont);
        /// <summary>
        /// “Backquote” or “quasiquote” expressions are useful for constructing a list or vector
        /// structure when most but not all of the desired structure is known in advance. If no
        /// commas appear within the template, the result of evaluating `template is equivalent
        /// (in the sense of equal?) to the result of evaluating ’template. If a comma appears
        /// within the template, however, the expression following the comma is evaluated (“unquoted”)
        /// and its result is inserted into the structure instead of the comma and the
        /// expression.
        /// </summary>
        private Bounce ApplyQuasiquoteSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            UnquoteHandler Unquoter = null;
            Unquoter = (pair, res, _env, _cont) =>
                {
                    return (Bounce)ApplyUnquoteSpecialForm (pair.Car, _env, x =>
                        {
                            res.Add (x as IExpression);
                            if (pair.Cdr is ScmEmptyList)
                                return (Bounce)cont (new ScmPair (res));
                            return (Bounce)Unquoter ((ScmPair)pair.Cdr, res, _env, _cont);
                        });
                };
            var quoteSpecialForm = expression as ScmQuasiquoteSpecialForm;
            var datum = quoteSpecialForm.Datum;
            if (datum is ScmPair)
                return Unquoter ((ScmPair)datum, new List<IExpression> (), env, cont);
            return ApplyUnquoteSpecialForm (datum, env, cont);
        }

        /// <summary>
        /// A helper method. See ApplyAndSpecialForm and ApplyOrSpecialForm.
        /// </summary>
        private Bounce ApplyAndOrSpecialForm (
            List<IExpression> expressionsList,
            IExpression defReturn,
            Func<IExpression, bool> pred,
            Environment env, Cont cont)
        {
            AndOrExpressionHandler ExpressionEvaluator = null;
            ExpressionEvaluator = (expressions, _env, _cont) =>
            {
                return Eval (expressions.First (), _env, evalResult =>
                {
                    if (pred ((IExpression)evalResult) || expressions.Tail ().IsEmpty ())
                        return (Bounce)_cont (evalResult);
                    return ExpressionEvaluator (expressions.Tail (), env, _cont);
                });
            };
            if (expressionsList.IsEmpty ())
                return (Bounce)cont (defReturn);
            return ExpressionEvaluator (expressionsList, env, cont);
        }

        private delegate Bounce AndOrExpressionHandler (List<IExpression> expressions, Environment env, Cont cont);
        /// <summary>
        /// The expressions are evaluated from left to right, and the value of the first expression
        /// that evaluates to a false value is returned. Any remaining expressions are not evaluated.
        /// If all the expressions evaluate to true values, the value of the last expression is
        /// returned. If there are no expressions then #t is returned.
        /// </summary>
        private Bounce ApplyAndSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var andSpecialForm = expression as ScmAndSpecialForm;
            return ApplyAndOrSpecialForm (
                andSpecialForm.Expressions,
                ScmTrueValue.Instance,
                x => (x is ScmFalseValue),
                env, cont);
        }

        /// <summary>
        /// The expressions are evaluated from left to right, and the value of the first expression
        /// that evaluates to a true value is returned. Any remaining expressions are not evaluated.
        /// If all expressions evaluate to false values, the value of the last expression is
        /// returned. If there are no expressions then #f is returned.
        /// </summary>
        private Bounce ApplyOrSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var orSpecialForm = expression as ScmOrSpecialForm;
            return ApplyAndOrSpecialForm (
                orSpecialForm.Expressions,
                ScmFalseValue.Instance,
                x => (x is ScmTrueValue || !(x is IBoolean)),
                env, cont);
        }

        /// <summary>
        /// Unquotes the exression.
        /// </summary>
        private Bounce ApplyUnquoteSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var unquoteSpecialForm = expression as ScmUnquoteSpecialForm;
            if (unquoteSpecialForm == null)
                return (Bounce)cont (expression);
            var datum = unquoteSpecialForm.Datum;
            if (datum is ScmPair)
                return Eval (new ScmCombination ((datum as ScmPair).GetImproperList ()), env, cont);
            return Eval (datum, env, cont);
        }

        /// <summary>
        /// (quote datum) evaluates to datum. Datum may be any external representation of a
        /// Scheme object
        /// </summary>
        private Bounce ApplyQuoteSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var quoteSpecialForm = expression as ScmQuoteSpecialForm;
            return (Bounce)cont (quoteSpecialForm.Datum);
        }

        private Bounce ApplySetSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var setSpecialForm = expression as ScmSetSpecialForm;
            return () => Eval (setSpecialForm.Expression, env, evalResult =>
            {
                env.Set (setSpecialForm.Name, (IExpression)evalResult);
                return (Bounce)cont (ScmUnassigned.Instance);
            });
        }

        private Bounce ApplyLambdaSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var lambdaSpecialForm = expression as ScmLambdaSpecialForm;
            return () => (Bounce)cont (new ScmClosure (lambdaSpecialForm, env));
        }
    }
}

