using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// A common procedure for one argument numeric operations for integers.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="env"></param>
        /// <param name="cont"></param>
        /// <returns></returns>
        private Bounce ApplyUnaryIntegerProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.HasOperands)
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                var evalResultAsInteger = evalResult as ScmIntegerNumber;
                if (evalResultAsInteger == null)
                    throw new EvaluatorException (UserMessages.ArgumentMustBeInteger, combination);
                if (combination.Procedure is ScmEvenPredicateProcedure || 
                    combination.Procedure is ScmOddPredicateProcedure)
                {
                    bool checkForEven = combination.Procedure is ScmEvenPredicateProcedure;
                    bool isEven = evalResultAsInteger.IsEven ();
                    bool result = isEven && checkForEven || !isEven && !checkForEven;
                    if (result)
                        return (Bounce)cont (ScmTrueValue.Instance);
                    return (Bounce)cont (ScmFalseValue.Instance);
                }
                throw new Exception ("Unknown operation in ApplyUnaryIntegerProcedure.");
            });
        }

        private Bounce ApplyBinaryIntegerProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.Operands.IsPair ())
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => EvalList (combination.Operands, env, evalResults =>
            {
                var evalResultsAsList = evalResults as List<IExpression>;
                var n1 = evalResultsAsList.First () as ScmIntegerNumber;
                var n2 = evalResultsAsList[1] as ScmIntegerNumber;
                if (n1 == null || n2 == null)
                    throw new EvaluatorException ("Arguments must evaluate to integers", combination);
                if ((combination.Procedure is ScmQuotientProcedure || 
                    combination.Procedure is ScmRemainderProcedure) && n2.IsZero ())
                    throw new EvaluatorException (UserMessages.CannotDivideByZero, combination);

                if (combination.Procedure is ScmQuotientProcedure)
                {
                    return (Bounce)cont (n1 / n2);
                }
                else if (combination.Procedure is ScmRemainderProcedure)
                {
                    return (Bounce)cont (n1 % n2);
                }
                else if (combination.Procedure is ScmModuloProcedure)
                {
                    var n3 = n1 % n2;
                    if (n1.IsNegative () ^ n2.IsNegative ())
                        n3 = n3 + n2;
                    return (Bounce)cont (n3);
                }
                throw new Exception ("Unknown operation in ApplyBinaryIntegerProcedure.");
            });
        }

        private Bounce ApplyAbsProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.HasOperands)
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                if (!(evalResult is INumber))
                    throw new EvaluatorException (UserMessages.ArgumentMustBeInteger, combination);
                if (evalResult is ScmDoubleNumber)
                    return (Bounce)cont (((ScmDoubleNumber)evalResult).Abs ());
                return (Bounce)cont (((ScmIntegerNumber)evalResult).Abs ());
            });
        }

        private Bounce ApplyCheckTypeProcedure<T> (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.HasOperands)
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                if (evalResult is T)
                    return (Bounce)cont (ScmTrueValue.Instance);
                return (Bounce)cont (ScmFalseValue.Instance);
            });
        }

        private Bounce ApplyRealPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            return ApplyCheckTypeProcedure<ScmDoubleNumber> (expression, env, cont);
        }

        private Bounce ApplyIntegerPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            return ApplyCheckTypeProcedure<ScmIntegerNumber> (expression, env, cont);
        }

        private Bounce ApplyNumberPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            return ApplyCheckTypeProcedure<INumber> (expression, env, cont);
        }

        private Bounce ApplyZeroPredicateProcedure (IExpression expression, Environment env, Cont cont)
        {
            var combination = expression as ScmCombination;
            if (!combination.HasOperands)
                throw new EvaluatorException (UserMessages.ExpectingOneArgument, combination);
            return () => Eval (combination.Operands.First (), env, evalResult =>
            {
                if (!(evalResult is INumber))
                    throw new EvaluatorException (UserMessages.ArgumentMustBeInteger, combination);
                bool isZero = false;
                if (evalResult is ScmDoubleNumber)
                {
                    isZero = ((ScmDoubleNumber)evalResult).IsZero ();
                }
                else if (evalResult is ScmIntegerNumber)
                {
                    isZero = ((ScmIntegerNumber)evalResult).IsZero ();
                }
                if (isZero)
                    return (Bounce)cont (ScmTrueValue.Instance);
                return (Bounce)cont (ScmFalseValue.Instance);
            });
        }

        private Bounce ApplyArithmeticProcedure (IExpression expression, Environment env, Cont cont)
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
                        var doubleArithmetic = m_NumericalOperations[typeof (ScmDoubleNumber)] as DoubleNumericalOperations;
                        return (Bounce)cont (doubleArithmetic.Accumulate (combination.Procedure as INumericalOperation, evalResultsAsList));
                    }
                    else
                    {
                        var integerArithmetic = m_NumericalOperations[typeof (ScmIntegerNumber)] as IntegerNumericalOperations;
                        return (Bounce)cont (integerArithmetic.Accumulate (combination.Procedure as INumericalOperation, evalResultsAsList));
                    }
                }
                catch (Exception e)
                {
                    throw new EvaluatorException (e.Message, combination);
                }
            });
        }
    }

    /// <summary>
    /// This class is used to execute numerical operations that can be applied to two
    /// or more arguments (except for + and * operation that accept zero arguments).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class NumericalOperations<T> : INumericalOperations where T : class, INumber
    {
        protected Dictionary<Type, Utils.AccumulateProc<T>> procArith = new Dictionary<Type, Utils.AccumulateProc<T>> ();
        protected Dictionary<Type, Utils.CompareProc<T>> procCmp = new Dictionary<Type, Utils.CompareProc<T>> ();

        protected virtual T AddProc (T a1, T a2)
        {
            return default (T);
        }
        protected virtual T SubProc (T a1, T a2)
        {
            return default (T);
        }
        protected virtual T MulProc (T a1, T a2)
        {
            return default (T);
        }
        protected virtual T DivProc (T a1, T a2)
        {
            return default (T);
        }

        protected virtual T MinProc (T a1, T a2)
        {
            return default (T);
        }

        protected virtual T MaxProc (T a1, T a2)
        {
            return default (T);
        }

        protected virtual bool LessThan (T a1, T a2)
        {
            return default (bool);
        }

        protected virtual bool GreaterThan (T a1, T a2)
        {
            return default (bool);
        }

        protected virtual bool EqualTo (T a1, T a2)
        {
            return default (bool);
        }

        protected virtual bool LessThanOrEqualTo (T a1, T a2)
        {
            return default (bool);
        }

        protected virtual bool GreaterThanOrEqualTo (T a1, T a2)
        {
            return default (bool);
        }

        protected abstract T GetZero ();
        protected abstract T GetOne ();

        public virtual IBoolean Compare (IComparisonOperation procName, List<INumber> operands)
        {
            var operandList = operands.ConvertAll (x => x as T);
            var accProc = procCmp[procName.GetType ()];
            var res = Utils.CompareList (accProc, operandList);
            if (res)
                return ScmTrueValue.Instance;
            return ScmFalseValue.Instance;
        }

        public T Accumulate (INumericalOperation procName, List<INumber> operands)
        {
            var operandList = operands.ConvertAll (x => x as T);

            var accProc = procArith[procName.GetType ()];
            var init = default (T);
            if (procName is ScmAddition)
            {
                init = GetZero ();
            }
            else if (procName is ScmMultiplication)
            {
                init = GetOne ();
            }
            else if (procName is ScmMinProcedure ||
                procName is ScmMaxProcedure)
            {
                if (operandList.Count < 2)
                    throw new Exception ("Expects at least 2 argument");
                init = null;
            }
            else
            {
                if (operandList.IsEmpty ())
                    throw new Exception ("Expects at least 1 argument");

                if (operandList.IsSingle ())
                {
                    if (procName is ScmDivision)
                        operandList.Insert (0, GetOne ());
                    else
                        operandList.Insert (0, GetZero ());
                }
                init = operandList.Head ();
                operandList = operandList.Tail ();
            }

            return Utils.AccumulateList (accProc, operandList, init);
        }

        public NumericalOperations ()
        {
            procArith.Add (typeof (ScmAddition), AddProc);
            procArith.Add (typeof (ScmSubtraction), SubProc);
            procArith.Add (typeof (ScmMultiplication), MulProc);
            procArith.Add (typeof (ScmDivision), DivProc);
            procArith.Add (typeof (ScmMinProcedure), MinProc);
            procArith.Add (typeof (ScmMaxProcedure), MaxProc);

            procCmp.Add (typeof (ScmLessThan), LessThan);
            procCmp.Add (typeof (ScmGreaterThan), GreaterThan);
            procCmp.Add (typeof (ScmEqualTo), EqualTo);
            procCmp.Add (typeof (ScmLessThanOrEqualTo), LessThanOrEqualTo);
            procCmp.Add (typeof (ScmGreaterThanOrEqualTo), GreaterThanOrEqualTo);
        }

        public virtual Utils.AccumulateProc<T> GetProc (INumericalOperation procName)
        {
            return procArith[procName.GetType ()];
        }

        public virtual Utils.CompareProc<T> GetProc (IComparisonOperation procName)
        {
            return procCmp[procName.GetType ()];
        }
    }

    class DoubleNumericalOperations : NumericalOperations<ScmDoubleNumber>
    {
        protected override ScmDoubleNumber AddProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return a1 + a2;
        }
        protected override ScmDoubleNumber SubProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return a2 - a1;
        }

        protected override ScmDoubleNumber MulProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return a1 * a2;
        }

        protected override ScmDoubleNumber DivProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return a2 / a1;
        }

        protected override ScmDoubleNumber MinProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            if (a2 == null)
                return a1;
            if (LessThan (a2, a1))
                return a2;
            return a1;
        }

        protected override ScmDoubleNumber MaxProc (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            if (a2 == null)
                return a1;
            if (GreaterThan (a2, a1))
                return a2;
            return a1;
        }

        protected override bool LessThan (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return (a1.Value < a2.Value);
        }

        protected override bool GreaterThan (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return (a1.Value > a2.Value);
        }

        protected override bool EqualTo (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return (a1.Value == a2.Value);
        }

        protected override bool LessThanOrEqualTo (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return (a1.Value <= a2.Value);
        }

        protected override bool GreaterThanOrEqualTo (ScmDoubleNumber a1, ScmDoubleNumber a2)
        {
            return (a1.Value >= a2.Value);
        }

        protected override ScmDoubleNumber GetOne ()
        {
            return new ScmDoubleNumber (1);
        }

        protected override ScmDoubleNumber GetZero ()
        {
            return new ScmDoubleNumber (0);
        }
    }

    class IntegerNumericalOperations : NumericalOperations<ScmIntegerNumber>
    {
        protected override ScmIntegerNumber AddProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return a1 + a2;
        }
        protected override ScmIntegerNumber SubProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return a2 - a1;
        }

        protected override ScmIntegerNumber MulProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return a1 * a2;
        }

        protected override ScmIntegerNumber DivProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return a2 / a1;
        }

        protected override bool LessThan (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return (a1.Value < a2.Value);
        }

        protected override bool GreaterThan (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return (a1.Value > a2.Value);
        }

        protected override bool EqualTo (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return (a1.Value == a2.Value);
        }

        protected override bool LessThanOrEqualTo (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return (a1.Value <= a2.Value);
        }

        protected override bool GreaterThanOrEqualTo (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            return (a1.Value >= a2.Value);
        }

        protected override ScmIntegerNumber MinProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            if (a2 == null)
                return a1;
            if (LessThan (a2, a1))
                return a2;
            return a1;
        }

        protected override ScmIntegerNumber MaxProc (ScmIntegerNumber a1, ScmIntegerNumber a2)
        {
            if (a2 == null)
                return a1;
            if (GreaterThan (a2, a1))
                return a2;
            return a1;
        }

        protected override ScmIntegerNumber GetOne ()
        {
            return new ScmIntegerNumber (1);
        }

        protected override ScmIntegerNumber GetZero ()
        {
            return new ScmIntegerNumber (0);
        }
    }
}
