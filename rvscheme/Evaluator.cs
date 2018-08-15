using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    /// <summary>
    /// The interpreter.
    /// </summary>
    sealed partial class Evaluator
    {
        // Continuation delegate; may return null or Cont iteself.
        public delegate Object Cont (Object e);
        // Bounce for trampolining.
        private delegate Bounce Bounce ();

        // We will thread our methods with an additional parameter for continuation
        // along environments and expressions. So we'll have each procedure to return a
        // Bounce closure, which either encapsulates a procedure call, in case when
        // the procedure wants to call other procedure, or it encapsulates continuation call
        // passing procedure result, that is when procedure wants to return something.
        // Each bouncing procedure will be designed in such a way that those Bounce 
        // returns are really all tail expressions.

        // This programming style, when procedure's results are passed to some other procedure
        // rather than returning it directly, is known as continuation passing style. Procedures
        // written using this style never return, they continuously pass their results
        // forward to other procedures, hence continuation. If they happen to be tail expressions
        // then they are tail called. This allows avoding of stack growing.

        // However, since C# isn't tail optimized, we need to use this "bouncing" trick
        // described above. This technique is known as trampolining.

        // Special form handlers are set up in a map for easy access.
        delegate Bounce SpecialFormHandler (IExpression proc, Environment env, Cont cont);
        Dictionary<Type, SpecialFormHandler> m_SFHandlers = new Dictionary<Type, SpecialFormHandler> ();

        // Primitive procedures are set up in a map for easy access as well.
        delegate Bounce PrimitiveProcedureHandler (IExpression proc, Environment env, Cont cont);
        Dictionary<Type, PrimitiveProcedureHandler> m_PPHandlers = new Dictionary<Type, PrimitiveProcedureHandler> ();

        private Dictionary<Type, INumericalOperations> m_NumericalOperations = new Dictionary<System.Type, INumericalOperations> ();
        private Environment m_Env = new Environment ();
        /// <summary>
        /// Initializes the interpreter with handlers etc.
        /// </summary>
        public Evaluator ()
        {
            // Set up arithmetic modules mapping.
            m_NumericalOperations.Add (typeof (ScmIntegerNumber), new IntegerNumericalOperations ());
            m_NumericalOperations.Add (typeof (ScmDoubleNumber), new DoubleNumericalOperations ());

            // Set up special form handlers.
            m_SFHandlers.Add (typeof (ScmIfSpecialForm), ApplyIfSpecialForm);
            m_SFHandlers.Add (typeof (ScmLetSpecialForm), ApplyLetSpecialForm);
            m_SFHandlers.Add (typeof (ScmLetrecSpecialForm), ApplyLetrecSpecialForm);
            m_SFHandlers.Add (typeof (ScmLambdaSpecialForm), ApplyLambdaSpecialForm);
            m_SFHandlers.Add (typeof (ScmDefineVarSpecialForm), ApplyDefineVarSpecialForm);
            m_SFHandlers.Add (typeof (ScmDefineProcSpecialForm), ApplyDefineProcSpecialForm);
            m_SFHandlers.Add (typeof (ScmSetSpecialForm), ApplySetSpecialForm);
            m_SFHandlers.Add (typeof (ScmCondSpecialForm), ApplyCondSpecialForm);
            m_SFHandlers.Add (typeof (ScmQuoteSpecialForm), ApplyQuoteSpecialForm);
            m_SFHandlers.Add (typeof (ScmQuasiquoteSpecialForm), ApplyQuasiquoteSpecialForm);
            m_SFHandlers.Add (typeof (ScmUnquoteSpecialForm), ApplyUnquoteSpecialForm);

            m_SFHandlers.Add (typeof (ScmBeginSpecialForm), ApplyBeginSpecialForm);

            m_SFHandlers.Add (typeof (ScmAndSpecialForm), ApplyAndSpecialForm);
            m_SFHandlers.Add (typeof (ScmOrSpecialForm), ApplyOrSpecialForm);

            // Set up primitive procedure handlers.
            m_PPHandlers.Add (typeof (ScmAddition), ApplyArithmeticProcedure);
            m_PPHandlers.Add (typeof (ScmSubtraction), ApplyArithmeticProcedure);
            m_PPHandlers.Add (typeof (ScmMultiplication), ApplyArithmeticProcedure);
            m_PPHandlers.Add (typeof (ScmDivision), ApplyArithmeticProcedure);
            m_PPHandlers.Add (typeof (ScmMinProcedure), ApplyArithmeticProcedure);
            m_PPHandlers.Add (typeof (ScmMaxProcedure), ApplyArithmeticProcedure);

            m_PPHandlers.Add (typeof (ScmZeroPredicateProcedure), ApplyZeroPredicateProcedure);
            m_PPHandlers.Add (typeof (ScmQuotientProcedure), ApplyBinaryIntegerProcedure);
            m_PPHandlers.Add (typeof (ScmRemainderProcedure), ApplyBinaryIntegerProcedure);
            m_PPHandlers.Add (typeof (ScmModuloProcedure), ApplyBinaryIntegerProcedure);
            m_PPHandlers.Add (typeof (ScmAbsProcedure), ApplyAbsProcedure);
            m_PPHandlers.Add (typeof (ScmOddPredicateProcedure), ApplyUnaryIntegerProcedure);
            m_PPHandlers.Add (typeof (ScmEvenPredicateProcedure), ApplyUnaryIntegerProcedure);
            m_PPHandlers.Add (typeof (ScmNumberPredicateProcedure), ApplyNumberPredicateProcedure);
            m_PPHandlers.Add (typeof (ScmRealPredicateProcedure), ApplyRealPredicateProcedure);
            m_PPHandlers.Add (typeof (ScmIntegerPredicateProcedure), ApplyIntegerPredicateProcedure);

            m_PPHandlers.Add (typeof (ScmLessThan), ApplyComparisonProcedure);
            m_PPHandlers.Add (typeof (ScmGreaterThan), ApplyComparisonProcedure);
            m_PPHandlers.Add (typeof (ScmEqualTo), ApplyComparisonProcedure);
            m_PPHandlers.Add (typeof (ScmLessThanOrEqualTo), ApplyComparisonProcedure);
            m_PPHandlers.Add (typeof (ScmGreaterThanOrEqualTo), ApplyComparisonProcedure);
            m_PPHandlers.Add (typeof (ScmConsProcedure), ApplyConsProcedure);
            m_PPHandlers.Add (typeof (ScmCarProcedure), ApplyPairSelectorProcedure);
            m_PPHandlers.Add (typeof (ScmCdrProcedure), ApplyPairSelectorProcedure);
            m_PPHandlers.Add (typeof (ScmSetCarProcedure), ApplyPairMutatorProcedure);
            m_PPHandlers.Add (typeof (ScmSetCdrProcedure), ApplyPairMutatorProcedure);
            m_PPHandlers.Add (typeof (ScmListProcedure), ApplyListProcedure);
            m_PPHandlers.Add (typeof (ScmListPredicateProcedure), ApplyListPredicateProcedure);
            m_PPHandlers.Add (typeof (ScmNullPredicateProcedure), ApplyNullPredicateProcedure);
            m_PPHandlers.Add (typeof (ScmLengthProcedure), ApplyLengthProcedure);
            m_PPHandlers.Add (typeof (ScmListRefProcedure), ApplyListRefProcedure);
            m_PPHandlers.Add (typeof (ScmCallCcProcedure), ApplyCallCcProcedure);
        }

        // FIXME: move inside of EvalList.
        private Bounce EvalList (List<IExpression> inExpressions, Environment env, Cont cont)
        {
            var outExpressions = new List<IExpression> ();
            return EvalList (inExpressions, outExpressions, env, cont);
        }

        /// <summary>
        /// Evaluates a list of expressions using the given environment; passes the results
        /// to the given continuation. This procedure works recursively, step-by-step, returning
        /// bounce object after each expression.
        /// </summary>
        /// <param name="inExpressions">A list of expressions to evaluate.</param>
        /// <param name="outExpressions">A list of resulting expressions.</param>
        /// <param name="env">Environment in which the given expressions will be evaluated.</param>
        /// <param name="cont">Continuation.</param>
        /// <returns>Bounce or null.</returns>
        private Bounce EvalList (List<IExpression> inExpressions, List<IExpression> outExpressions, Environment env, Cont cont)
        {
            System.Diagnostics.Debug.Assert (outExpressions != null);
            // Evaluates the first element in the list, adds it to the outExpression, and then
            // recursively calls EvalList to eval the rest of the results. Recursive calls are
            // made from within the continuation created here, in the method. The original continuation,
            // that is to which the result will be passed, is passed along environment each
            // time a recursive call happens. When there's nothing to evaluate more, the original continuation
            // will be used to pass the results forward.
            if (!inExpressions.IsEmpty ())
                return () => Eval (inExpressions.Head (), env, evaluatedExpression =>
                {
                    outExpressions.Add ((IExpression)evaluatedExpression);
                    return EvalList (inExpressions.Tail (), outExpressions, env, cont);
                });
            return () => (Bounce)cont (outExpressions);
        }

        /// <summary>
        /// Evaluates combination.
        /// </summary>
        /// <param name="combination">A combination to evaluate.</param>
        /// <param name="env">Environment in which the given combination will be evaluated.</param>
        /// <param name="cont">Continuation.</param>
        /// <returns>Bounce ir null.</returns>
        private Bounce EvalCombination (ScmCombination combination, Environment env, Cont cont)
        {
            System.Diagnostics.Debug.Assert (combination != null);

            // If this is an empty combination, then signal an error, since there is no
            // procedure to apply.
            if (combination.IsEmpty)
                throw new EvaluatorException (UserMessages.CannotApplyProcedure, combination);
            // Otherwise evaluate the procedure.
            return () => Eval (combination.Procedure, env, evalResult =>
            {
                // Now check whether the expression that is supposed to evaluate to a
                // procedure has really evaluated to a procedure, and signal an error
                // otherwise.
                var procedure = evalResult as IProcedure;
                if (procedure == null)
                    throw new EvaluatorException (UserMessages.CannotApplyProcedure, combination);
                // Depending on whether we are dealing with a primitive procedure or
                // just a procedure we call different handlers.
                if ((procedure is IPrimitiveProcedure))
                {
                    combination.Expressions[0] = procedure;
                    return ApplyPrimitiveProcedure (combination, env, cont);
                }
                // TODO: this assingment needs fixing.
                combination.Expressions[0] = procedure;
                return ApplyProcedure (combination, env, cont);
            });
        }

        /// <summary>
        /// Evaluates a symbol by just looking up in an environment.
        /// Signals an error if a symbol cannot be found.
        /// </summary>
        /// <param name="symbol">A symbol to look up.</param>
        /// <param name="env">Environment in which the given symbol will be looked up.</param>
        /// <param name="cont">Continuation.</param>
        /// <returns>Bounce or null.</returns>
        private Bounce EvalSymbol (ScmSymbol symbol, Environment env, Cont cont)
        {
            var evalResult = env.Get (symbol);
            if (evalResult is ScmUnassigned)
            {
                throw new EvaluatorException (String.Format (UserMessages.SymbolIsUnassigned, symbol.Token.AsString), symbol);
            }
            return () => (Bounce)cont (evalResult);
        }

        /// <summary>
        /// Evaluates a special form.
        /// </summary>
        /// <param name="specialForm">Special form</param>
        /// <param name="env">Environment in which the given special form will be evaluated.</param>
        /// <param name="cont">Continuation.</param>
        /// <returns>Bounce or null.</returns>
        private Bounce EvalSpecialForm (ISpecialForm specialForm, Environment env, Cont cont)
        {
            // Get the type of a special form and dispatch on it.
            return () => m_SFHandlers[specialForm.GetType ()] (specialForm, env, cont);
        }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <param name="env">Environment in which the given expression will be evaluated.</param>
        /// <param name="cont">Continuation.</param>
        /// <returns>Bounce or null.</returns>
        private Bounce Eval (IExpression expression, Environment env, Cont cont)
        {
            if (expression is ScmCombination)
            {
                return () => EvalCombination ((ScmCombination)expression, env, cont);
            }
            else if (expression is ScmSymbol)
            {
                return () => EvalSymbol ((ScmSymbol)expression, env, cont);
            }
            else if (expression is ISpecialForm)
            {
                return () => EvalSpecialForm ((ISpecialForm)expression, env, cont);
            }
            return () => (Bounce)cont (expression);
        }

        /// <summary>
        /// Executes each expression in the list of expressions and returns
        /// evaluated result for each expression through cont parameter.
        /// </summary>
        /// <param name="program">A list of expressions.</param>
        /// <param name="cont">Continuation.</param>
        public void Execute (List<IExpression> program, Cont cont)
        {
            foreach (var expr in program)
            {
                Bounce r = () => Eval (expr, m_Env, cont);
                while (r != null)
                    r = r ();
            }
        }
    }
}
