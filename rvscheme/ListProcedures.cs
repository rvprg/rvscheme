using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        private Bounce ApplyLengthProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                if (evalResult is ScmEmptyList)
                    return (Bounce)cont (new ScmIntegerNumber (0));
                var evalResultAsList = evalResult as ScmPair;
                if (evalResultAsList == null)
                    throw new EvaluatorException (UserMessages.ArgumentMustBeProperList, combination);
                if (!evalResultAsList.IsProperList)
                    throw new EvaluatorException (UserMessages.ArgumentMustBeProperList, combination);
                var length = evalResultAsList.Length;
                return (Bounce)cont (new ScmIntegerNumber (length));
            });
        }

        private Bounce ApplyListRefProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsPair ())
                throw new EvaluatorException (UserMessages.ExpectingTwoArguments, combination);
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = evalResults as List<IExpression>;
                var refList = evalResultsAsList.First () as ScmPair;
                if (refList == null)
                    throw new EvaluatorException (UserMessages.ArgumentMustBeList, combination);
                var refIndex = evalResultsAsList[1] as ScmIntegerNumber;
                if (refIndex == null)
                    throw new EvaluatorException (UserMessages.ArgumentMustBeInteger, combination);

                int k = (int)refIndex.Value;
                if (k < 0 || k > refList.Length - 1)
                    throw new EvaluatorException (UserMessages.IndexIsOutOfRange, combination);
                return (Bounce)cont (refList.GetAt (k));
            });
        }

        private Bounce ApplyNullPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                if (evalResult is ScmEmptyList)
                    return (Bounce)cont (ScmTrueValue.Instance);
                return (Bounce)cont (ScmFalseValue.Instance);
            });
        }

        private Bounce ApplyListPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                var evalResultAsList = evalResult as ScmPair;
                bool isProperList = (evalResult is ScmEmptyList) || evalResultAsList != null && evalResultAsList.IsProperList;
                if (isProperList)
                    return (Bounce)cont (ScmTrueValue.Instance);
                return (Bounce)cont (ScmFalseValue.Instance);
            });
        }

        private Bounce ApplyListProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = evalResults as List<IExpression>;
                if (evalResultsAsList.IsEmpty ())
                    return (Bounce)cont (ScmEmptyList.Instance);
                return (Bounce)cont (new ScmPair (evalResultsAsList));
            });
        }

        private Bounce ApplyConsProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsPair ())
                throw new EvaluatorException (UserMessages.ExpectingTwoArguments, combination);
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = evalResults as List<IExpression>;
                return (Bounce)cont (new ScmPair (evalResultsAsList[0], evalResultsAsList[1]));
            });
        }

        private Bounce ApplyPairMutatorProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsPair ())
                throw new EvaluatorException (UserMessages.ExpectingTwoArguments, combination);
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = (evalResults as List<IExpression>);
                var scmPair = evalResultsAsList.First () as ScmPair;
                if (scmPair == null)
                    throw new EvaluatorException (UserMessages.FirstArgMustBePair, combination);
                var scmNewValue = evalResultsAsList[1];
                if (combination.Procedure is ScmSetCarProcedure)
                    scmPair.Car = scmNewValue;
                else
                    scmPair.Cdr = scmNewValue;
                return (Bounce)cont (ScmUnassigned.Instance);
            });
        }

        private Bounce ApplyPairSelectorProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsSingle ())
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var firstOperand = (evalResults as List<IExpression>).First ();
                if (firstOperand is ScmEmptyList || !(firstOperand is ScmPair))
                    throw new EvaluatorException (UserMessages.IllegalDatum, combination);
                var firstOperandAsList = firstOperand as ScmPair;
                if (combination.Procedure is ScmCarProcedure)
                    return (Bounce)cont (firstOperandAsList.Car);
                return (Bounce)cont (firstOperandAsList.Cdr);
            });
        }
    }
}
