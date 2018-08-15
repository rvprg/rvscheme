using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// This is a helper delegate; used in ApplyCondSpecialForm.
        /// </summary>
        private delegate Bounce ClauseHandler (List<ScmCombination> clauses, Environment env, Cont cont);
        /// <summary>
        /// This method evaluates a cond special form. A cond must have at least one
        /// clause. This method evaluates each clause one by one, sequentially, and if the 
        /// predicate in a clause evaluates to #t, then corresponding expressions are evaluated 
        /// sequentially and the result of the last of the expressions gets returned.
        /// </summary>
        private Bounce ApplyCondSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var condSpecialForm = expression as ScmCondSpecialForm;
            // Since we write everything in CPS style, we define this helper closure, which will
            // evaluate clauses recursively, one by one.
            ClauseHandler ClauseEvaluator = null;
            ClauseEvaluator = (clauses, _env, _cont) =>
            {
                // We return unassigned value if there is nothing to evaluate.
                if (clauses.IsEmpty ())
                    return (Bounce)_cont (ScmUnassigned.Instance);
                // We get a clause and check whether it is not empty.
                var clause = clauses.Head ();
                if (clause.IsEmpty)
                    throw new EvaluatorException (UserMessages.ClauseCannotBeEmptyList, condSpecialForm);
                // Now we evaluate the predicate.
                return Eval (clause.Expressions.First (), _env, evalResult =>
                {
                    // If it evaluates to #t, or returns any other object that is not
                    // #f, then we evaluate corresponding expressions.
                    if (evalResult is ScmTrueValue || !(evalResult is IBoolean))
                    {
                        // Since the predicate evaluated to #t, we evaluate expressions then.
                        // NOTE: we might need to evaluate the last expression separately.
                        return EvalList (clause.Expressions.Tail (), _env, evalResults =>
                        {
                            var evalResultsAsList = (List<IExpression>)evalResults;
                            // If there was nothing to evaluate, return the value of
                            // the predicate.
                            if (evalResultsAsList.IsEmpty ())
                                return (Bounce)_cont (evalResult);
                            // Otherwise, return the result of the last expression.
                            return (Bounce)_cont (evalResultsAsList.Last ());
                        });
                    }
                    // Predicate in the clause did not evaluate to #t, so process with
                    // the next clause.
                    return ClauseEvaluator (clauses.Tail (), _env, _cont);
                });
            };
            // Evaluate clauses one by one in a recursive manner.
            return ClauseEvaluator (condSpecialForm.Clauses, env, cont);
        }

        /// <summary>
        /// This method evaluates the if special form. The special form is rather simple
        /// to evaluate: if the predicate evaluates to #t, or evaluates to any other
        /// object that is not #f, we evaluate the consequent, otherwise the alternative.
        /// If there's no alternative, we return the unassigned value.
        /// </summary>
        private Bounce ApplyIfSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var ifSpecialForm = expression as ScmIfSpecialForm;
            // First evaluate the predicate.
            return () => Eval (ifSpecialForm.Predicate, env, evalResult =>
                {
                    // If it evaluates to #t, then evaluate the consequent.
                    if (evalResult is ScmTrueValue || !(evalResult is IBoolean))
                    {
                        return Eval (ifSpecialForm.Consequent, env, cont);
                    }
                    // Otherwise we return undefined (the default value of the alternative).
                    return Eval (ifSpecialForm.Alternative, env, cont);
                });
        }
    }
}
