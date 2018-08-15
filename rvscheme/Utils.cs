using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    /// <summary>
    /// This class contains various helper methods and a number of extension
    /// methods.
    /// </summary>
    static class Utils
    {
        public static List<T> AllButLast<T> (this List<T> t)
        {
            return t.GetRange (0, t.Count - 1);
        }

        public static bool IsPair<T> (this List<T> t)
        {
            return t.Count == 2;
        }

        public static bool IsSingle<T> (this List<T> t)
        {
            return t.Count == 1;
        }

        public static bool IsEmpty<T> (this List<T> t)
        {
            return t.Count == 0;
        }

        public static T OneBeforeLast<T> (this List<T> t)
        {
            return t[t.Count - 2];
        }

        public static T Last<T> (this List<T> t)
        {
            return t[t.Count - 1];
        }

        public static T First<T> (this List<T> t)
        {
            return t.Head ();
        }

        public static T Head<T> (this List<T> t)
        {
            return t[0];
        }

        public static List<T> TailFrom<T> (this List<T> t, int s)
        {
            return t.GetRange (s, t.Count - s);
        }

        public static List<T> Tail<T> (this List<T> t)
        {
            return t.TailFrom (1);
        }

        public delegate T AccumulateProc<T> (T a1, T a2);
        public static T AccumulateList<T> (AccumulateProc<T> proc, List<T> list, T init)
        {
            foreach (var v in list)
                init = proc (v, init);
            return init;
        }

        public delegate bool CompareProc<T> (T a1, T a2);
        public static bool CompareList<T> (CompareProc<T> proc, List<T> list)
        {
            bool res = true;
            for (int i = 0; i < list.Count - 1; ++i)
                res = res && proc (list[i], list[i + 1]);
            return res;
        }

        public static bool IsInteger (string str)
        {
            System.Numerics.BigInteger intOut = 0;
            return ParseInteger (str, out intOut);
        }

        public static bool ParseInteger (string str, out System.Numerics.BigInteger intOut)
        {
            intOut = 0;
            bool isInteger = System.Numerics.BigInteger.TryParse (str, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out intOut);
            if (isInteger)
                return true;
            return false;
        }

        public static bool IsDouble (string str)
        {
            double intOut = 0;
            return ParseDouble (str, out intOut);
        }

        public static bool ParseDouble (string str, out double dblOut)
        {
            dblOut = 0;
            bool isDouble = double.TryParse (str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out dblOut);
            if (isDouble)
                return true;
            return false;
        }

        public static bool IsNumber (string str)
        {
            return IsInteger (str) || IsDouble (str);
        }

        public static List<INumber> ConvertNumbers (List<IExpression> operands)
        {
            bool allDoubles = false;
            var resultsAsNumbers = new List<INumber> ();
            foreach (IExpression exp in operands)
            {
                if (!(exp is INumber))
                {
                    throw new Exception (String.Format ("Expects numbers as arguments, but was given {0}", exp));
                }

                if (exp is ScmDoubleNumber)
                {
                    allDoubles = true;
                    for (int i = 0; i < resultsAsNumbers.Count; ++i)
                    {
                        if (resultsAsNumbers[i] is ScmIntegerNumber)
                        {
                            var expInt = resultsAsNumbers[i] as ScmIntegerNumber;
                            resultsAsNumbers[i] = (ScmDoubleNumber)expInt;
                        }
                    }
                }

                if (exp is ScmIntegerNumber && allDoubles)
                {
                    var expInt = exp as ScmIntegerNumber;
                    resultsAsNumbers.Add ((ScmDoubleNumber)expInt);
                }
                else
                    resultsAsNumbers.Add (exp as INumber);
            }
            return resultsAsNumbers;
        }
    }
}
