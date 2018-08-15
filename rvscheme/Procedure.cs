using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        private Bounce ApplyPrimitiveProcedure (ScmCombination combination, Environment env, Cont cont)
        {
            return () => m_PPHandlers[combination.Procedure.GetType ()] (combination, env, cont);
        }

        private Bounce ApplyProcedure (ScmCombination combination, Environment env, Cont cont)
        {
            if (combination.Procedure is ScmEvalProcedure)
            {
                return () => ApplyEvalProcedure (combination, env, cont);
            }
            else if (combination.Procedure is ScmClosure)
            {
                return () => ApplyClosureProcedure (combination, env, cont);
            }
            else if (combination.Procedure is ScmContinuation)
            {
                return () => ApplyContinuationProcedure (combination, env, cont);
            }
            throw new EvaluatorException ("Unknown procedure", combination);
        }

        private Bounce ApplyEvalProcedure (ScmCombination combination, Environment env, Cont cont)
        {
            if (!combination.Operands.IsSingle ())
                throw new EvaluatorException ("Expecting 1 argument", combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                var evalResultAsList = evalResult as ScmPair;
                if (evalResultAsList == null)
                    throw new EvaluatorException ("Must be a non-empty list", combination);

                var combinationFromImproperList = new ScmCombination (evalResultAsList.GetImproperList ());
                return Eval (combinationFromImproperList, env, combinationEvalResult => { return (Bounce)cont (combinationEvalResult); });
            });
        }

        private Bounce ApplyCallCcProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
            {
                throw new EvaluatorException ("Expects 1 argument", expression);
            }

            return () => Eval (combination.Operands.First (), env, closure =>
            {
                var oneArgumentClosure = closure as ScmClosure;
                if (oneArgumentClosure == null)
                {
                    throw new EvaluatorException ("Argument for call/cc must evaluate to a procedure", expression);
                }
                if (!oneArgumentClosure.ArgumentList.IsSingle ())
                {
                    throw new EvaluatorException ("Call/cc argument must be a procedure of one argument", expression);
                }

                var currentContinuation = new ScmContinuation (cont);
                var closureCall = new ScmCombination (new List<IExpression> { oneArgumentClosure, currentContinuation });
                return ApplyClosureProcedure (closureCall, env, cont);
            });
        }

        private Bounce ApplyComparisonProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = new List<INumber> ();
                try
                {
                    evalResultsAsList = Utils.ConvertNumbers (evalResults as List<IExpression>);
                }
                catch (Exception e)
                {
                    throw new EvaluatorException (e.Message, combination);
                }
                bool isDoubleArithmetic = !evalResultsAsList.IsEmpty () && evalResultsAsList.Head () is ScmDoubleNumber;

                try
                {
                    if (isDoubleArithmetic)
                    {
                        var doubleComparison = m_NumericalOperations[typeof (ScmDoubleNumber)] as DoubleNumericalOperations;
                        return (Bounce)cont (doubleComparison.Compare (combination.Procedure as IComparisonOperation, evalResultsAsList));
                    }
                    else
                    {
                        var integerComparison = m_NumericalOperations[typeof (ScmIntegerNumber)] as IntegerNumericalOperations;
                        return (Bounce)cont (integerComparison.Compare (combination.Procedure as IComparisonOperation, evalResultsAsList));
                    }
                }
                catch (Exception e)
                {
                    throw new EvaluatorException (e.Message, combination);
                }
            });
        }

        private Bounce ApplyContinuationProcedure (ScmCombination combination, Environment env, Cont cont)
        {
            var continuation = combination.Procedure as ScmContinuation;
            if (!combination.HasOperands)
            {
                return (Bounce)continuation.Cont (ScmUnassigned.Instance);
            }
            else if (combination.Operands.IsSingle ())
            {
                return () => Eval (combination.Operands.First (), env, returnValue => (Bounce)continuation.Cont (returnValue));
            }
            throw new EvaluatorException ("Continuation expects zero or one argument", combination);
        }

        private Bounce ApplyClosureProcedure (ScmCombination combination, Environment env, Cont cont)
        {
            //((lambda (b . a) a) 1)
            var closure = combination.Procedure as ScmClosure;
            var numberOfArguments = closure.ArgumentList.Count;
            var operands = new List<IExpression> (combination.Operands);

            if (!closure.Dotted && numberOfArguments != operands.Count)
            {
                throw new EvaluatorException (String.Format ("Expecting {0} argument{1}", numberOfArguments, numberOfArguments > 1 ? "s" : ""), combination);
            }
            else if (closure.Dotted)
            {
                var numberOfArgumentsButLast = numberOfArguments - 1;
                if (numberOfArgumentsButLast > operands.Count)
                {
                    throw new EvaluatorException (String.Format ("Expecting at least {0} argument{1}", numberOfArgumentsButLast, numberOfArgumentsButLast > 1 ? "s" : ""), combination);
                }
                else if (numberOfArgumentsButLast == operands.Count)
                {
                    operands.Add (ScmEmptyList.Instance);
                }
                else
                {
                    var lastArgument = new ScmPair (operands.TailFrom (numberOfArgumentsButLast));
                    operands = new List<IExpression> (operands.GetRange (0, numberOfArgumentsButLast));
                    operands.Add (lastArgument);
                }
            }

            return () => EvalList (operands, env, evalResults =>
            {
                var environmentFrame = new EnvironmentFrame ();
                var evalResultsAsList = evalResults as List<IExpression>;
                for (int i = 0; i < closure.ArgumentList.Count; ++i)
                {
                    var argumentName = closure.ArgumentList[i];
                    var argumentValue = evalResultsAsList[i];
                    environmentFrame.Set (argumentName, argumentValue);
                }

                var newEnvironment = (new Environment (closure.Env)).Extend (environmentFrame);
                return EvalList (closure.Body.AllButLast (), newEnvironment, bodyEvalResults =>
                {
                    return Eval (closure.Body.Last (), newEnvironment, cont);
                });
            });
        }
    }
}

