using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace rvscheme
{
    sealed class ScmConsProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: cons]";
        }
    }

    sealed class ScmSetCarProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: set-car!]";
        }
    }

    sealed class ScmSetCdrProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: set-cdr!]";
        }
    }

    sealed class ScmCarProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: car]";
        }
    }

    sealed class ScmCdrProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: cdr]";
        }
    }

    sealed class ScmListPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: list?]";
        }
    }

    sealed class ScmListRefProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: list-ref]";
        }
    }

    sealed class ScmNullPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: list?]";
        }
    }

    sealed class ScmListProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: null?]";
        }
    }

    sealed class ScmLengthProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: length]";
        }
    }

    sealed class ScmEqualTo : IComparisonOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: =]";
        }
    }

    sealed class ScmLessThanOrEqualTo : IComparisonOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: <=]";
        }
    }

    sealed class ScmGreaterThanOrEqualTo : IComparisonOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: >=]";
        }
    }

    sealed class ScmGreaterThan : IComparisonOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: >]";
        }
    }

    sealed class ScmAndSpecialForm : ISpecialForm
    {
        List<IExpression> m_Expressions = null;

        public List<IExpression> Expressions
        {
            get { return m_Expressions; }
        }

        public ScmAndSpecialForm (List<IExpression> expressions)
        {
            m_Expressions = expressions;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: and]";
        }
    }

    sealed class ScmOrSpecialForm : ISpecialForm
    {
        List<IExpression> m_Expressions = null;

        public List<IExpression> Expressions
        {
            get { return m_Expressions; }
        }

        public ScmOrSpecialForm (List<IExpression> expressions)
        {
            m_Expressions = expressions;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: or]";
        }
    }

    sealed class ScmLessThan : IComparisonOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: <]";
        }
    }

    sealed class ScmAbsProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: abs]";
        }
    }

    sealed class ScmMaxProcedure : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: max]";
        }
    }

    sealed class ScmMinProcedure : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: min]";
        }
    }

    sealed class ScmModuloProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: modulo]";
        }
    }

    sealed class ScmRemainderProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: remainder]";
        }
    }

    sealed class ScmQuotientProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: quotient]";
        }
    }

    sealed class ScmRealPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: real?]";
        }
    }

    sealed class ScmIntegerPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: integer?]";
        }
    }

    sealed class ScmOddPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: odd?]";
        }
    }

    sealed class ScmNumberPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: number?]";
        }
    }

    sealed class ScmEvenPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: even?]";
        }
    }

    sealed class ScmZeroPredicateProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: zero?]";
        }
    }

    sealed class ScmCallCcProcedure : IPrimitiveProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: call/cc]";
        }
    }

    sealed class ScmPair : IExpression
    {
        public IExpression Car
        {
            get;
            set;
        }

        public IExpression Cdr
        {
            get;
            set;
        }

        public ScmPair (IExpression car, IExpression cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        private ScmPair ()
        {
        }

        private IEnumerable<IExpression> GetIterator ()
        {
            IExpression curr = this;
            while (curr is ScmPair)
            {
                yield return curr;
                curr = ((ScmPair)curr).Cdr;
                if (curr == this)
                    break;
            }
            yield return curr;
        }

        private void ForEachPair (Action<IExpression> action)
        {
            foreach (var pair in GetIterator ())
                action (pair);
        }

        private IExpression FindPair (Predicate<IExpression> predicate)
        {
            foreach (var pair in GetIterator ())
            {
                if (predicate (pair))
                    return pair;
            }
            return null;
        }

        public ScmPair (List<IExpression> list)
        {
            System.Diagnostics.Debug.Assert (list != null && list.Count > 0);
            ScmPair currPair = this;
            for (int i = 0; i < list.Count; ++i)
            {
                currPair.Car = list[i];
                if (i < list.Count - 1)
                {
                    currPair.Cdr = new ScmPair ();
                    currPair = (ScmPair)currPair.Cdr;
                }
                else
                    currPair.Cdr = ScmEmptyList.Instance;
            }
        }

        public int Length
        {
            get
            {
                int count = 0;
                IExpression expr = null;
                ForEachPair (x => { expr = x; count++; });
                if (expr == this || !(expr is ScmEmptyList))
                    return -1;
                return count - 1;
            }
        }

        public IExpression GetAt (int k)
        {
            int count = 0;
            var pair = FindPair (x => { count++; return count == k + 1; });
            if (pair is ScmPair)
                return (pair as ScmPair).Car;
            throw new EvaluatorException ("Failed on getting an indexed element", this);
        }

        public bool IsProperList
        {
            get
            {
                return Length != -1;
            }
        }

        public List<IExpression> GetImproperList ()
        {
            var improperList = new List<IExpression> ();
            ForEachPair (x => { if (x is ScmPair) improperList.Add ((x as ScmPair).Car); });
            return improperList;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[list]";
        }
    }

    sealed class ScmClosure : IProcedure
    {
        AbstractLambda m_Lambda = null;
        Environment m_Env = null;

        public bool Dotted
        {
            get { return m_Lambda.Dotted; }
        }

        public List<ScmSymbol> ArgumentList
        {
            get { return m_Lambda.Formals; }
        }

        public List<IExpression> Body
        {
            get { return m_Lambda.Expressions; }
        }

        public Environment Env
        {
            get { return m_Env; }
        }

        public ScmClosure (AbstractLambda lambda, Environment env)
        {
            m_Lambda = lambda;
            m_Env = new Environment (env);
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            string parameterList = !ArgumentList.IsEmpty () ? ArgumentList.Count.ToString () : "parameterless";
            return String.Format ("#[closure: {0}]", parameterList);
        }
    }

    sealed class ScmEvalProcedure : IProcedure
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: eval]";
        }
    }

    abstract class AbstractLambda : ISpecialForm
    {
        bool m_Dotted = false;
        List<ScmSymbol> m_Formals = null;
        List<IExpression> m_Expressions = null;

        public bool Dotted
        {
            get { return m_Dotted; }
        }

        public List<IExpression> Expressions
        {
            get { return this.m_Expressions; }
        }

        public List<ScmSymbol> Formals
        {
            get { return this.m_Formals; }
        }

        public AbstractLambda (List<ScmSymbol> f, List<IExpression> e, bool dotted)
        {
            m_Formals = f;
            m_Expressions = e;
            m_Dotted = dotted;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: lambda]";
        }
    }

    sealed class ScmLambdaSpecialForm : AbstractLambda
    {
        public ScmLambdaSpecialForm (List<ScmSymbol> f, List<IExpression> e, bool dotted)
            : base (f, e, dotted)
        {
        }

        public new string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: lambda]";
        }
    }

    sealed class ScmDefineProcSpecialForm : AbstractLambda
    {
        ScmSymbol m_ProcedureName = null;

        public ScmSymbol ProcedureName
        {
            get { return this.m_ProcedureName; }
        }

        public ScmDefineProcSpecialForm (ScmSymbol n, List<ScmSymbol> f, List<IExpression> e, bool dotted)
            : base (f, e, dotted)
        {
            m_ProcedureName = n;
        }

        public new string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: define]";
        }
    }

    sealed class ScmDefineVarSpecialForm : ISpecialForm
    {
        ScmSymbol m_Name = null;
        IExpression m_Expression = null;

        public ScmSymbol Name
        {
            get { return this.m_Name; }
        }

        public IExpression Expression
        {
            get { return this.m_Expression; }
        }

        public ScmDefineVarSpecialForm (ScmSymbol n, IExpression e)
        {
            m_Name = n;
            m_Expression = e;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: define]";
        }
    }

    sealed class ScmSetSpecialForm : ISpecialForm
    {
        ScmSymbol m_Name = null;
        IExpression m_Expression = null;

        public ScmSymbol Name
        {
            get { return this.m_Name; }
        }

        public IExpression Expression
        {
            get { return this.m_Expression; }
        }

        public ScmSetSpecialForm (ScmSymbol n, IExpression e)
        {
            m_Name = n;
            m_Expression = e;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: set]";
        }
    }

    abstract class AbstractLet : ISpecialForm
    {
        Dictionary<IExpression, IExpression> m_Bindings = null;
        List<IExpression> m_Expressions = null;

        public Dictionary<IExpression, IExpression> Bindings
        {
            get { return this.m_Bindings; }
        }

        public List<IExpression> Expressions
        {
            get { return this.m_Expressions; }
        }

        public AbstractLet (Dictionary<IExpression, IExpression> b, List<IExpression> e)
        {
            m_Bindings = b;
            m_Expressions = e;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: abstract let]";
        }
    }

    sealed class ScmLetrecSpecialForm : AbstractLet
    {
        public ScmLetrecSpecialForm (Dictionary<IExpression, IExpression> b, List<IExpression> e)
            : base (b, e)
        {
        }
    }

    sealed class ScmLetSpecialForm : AbstractLet
    {
        public ScmLetSpecialForm (Dictionary<IExpression, IExpression> b, List<IExpression> e)
            : base (b, e)
        {
        }
    }

    sealed class ScmCondSpecialForm : ISpecialForm
    {
        List<ScmCombination> m_Clauses;

        public List<ScmCombination> Clauses
        {
            get { return m_Clauses; }
        }

        public void AddClause (ScmCombination combination)
        {
            m_Clauses.Add (combination);
        }

        public ScmCondSpecialForm ()
        {
            m_Clauses = new List<ScmCombination> ();
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: cond]";
        }
    }

    sealed class ScmBeginSpecialForm : ISpecialForm
    {
        List<IExpression> m_Expressions = null;

        public List<IExpression> Expressions
        {
            get
            {
                return m_Expressions;
            }
        }

        public ScmBeginSpecialForm (List<IExpression> e)
        {
            m_Expressions = e;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: begin]";
        }
    }

    abstract class AbstractQuotation : ISpecialForm
    {
        protected IExpression m_Datum = null;
        public IExpression Datum
        {
            get
            {
                return this.m_Datum;
            }
        }

        public AbstractQuotation (IExpression e)
        {
            m_Datum = e;
        }

        public abstract string ToString (string format, IFormatProvider formatProvider);
    }

    sealed class ScmQuasiquoteSpecialForm : AbstractQuotation
    {
        public ScmQuasiquoteSpecialForm (IExpression e)
            : base (e)
        {
        }

        public override string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: quasiquote]";
        }
    }

    sealed class ScmQuoteSpecialForm : AbstractQuotation
    {
        public ScmQuoteSpecialForm (IExpression e)
            : base (e)
        {
        }

        public override string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: quote]";
        }
    }

    sealed class ScmUnquoteSpecialForm : AbstractQuotation
    {
        public ScmUnquoteSpecialForm (IExpression e)
            : base (e)
        {
        }

        public override string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: unquote]";
        }
    }

    sealed class ScmIfSpecialForm : ISpecialForm
    {
        IExpression m_Predicate = null;
        IExpression m_Consequent = null;
        IExpression m_Alternative = null;

        public IExpression Alternative
        {
            get { return this.m_Alternative; }
        }

        public IExpression Consequent
        {
            get { return this.m_Consequent; }
        }

        public IExpression Predicate
        {
            get { return this.m_Predicate; }
        }

        public ScmIfSpecialForm (IExpression p, IExpression c, IExpression a)
        {
            m_Predicate = p;
            m_Consequent = c;
            m_Alternative = a;
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[special form: if]";
        }
    }

    sealed class ScmAddition : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: +]";
        }
    }

    sealed class ScmSubtraction : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: -]";
        }
    }

    sealed class ScmMultiplication : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: *]";
        }
    }

    sealed class ScmDivision : INumericalOperation
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[primitive procedure: /]";
        }
    }

    abstract class ScmAbstractExpression 
    {
        private Token m_Token;
        public Token Token
        {
            get { return m_Token; }
        }

        public ScmAbstractExpression (Token token)
        {
            m_Token = token;
        }
    }

    sealed class ScmCombination : ScmAbstractExpression, IExpression
    {
        private List<IExpression> m_Expressions;
        public List<IExpression> Expressions
        {
            get { return m_Expressions; }
        }

        public bool IsEmpty
        {
            get { return m_Expressions.IsEmpty (); }
        }

        public bool HasOperands
        {
            get
            {
                return (!IsEmpty && !Operands.IsEmpty ());
            }
        }

        public IExpression Procedure
        {
            get { return m_Expressions.Head (); }
        }

        public List<IExpression> Operands
        {
            get
            {
                if (m_Expressions.Count > 1)
                    return m_Expressions.Tail ();
                return new List<IExpression> ();
            }
        }

        public ScmCombination (Token token)
            : base (token)
        {
            m_Expressions = new List<IExpression> ();
        }

        public ScmCombination (List<IExpression> exprList)
            : this (exprList, new Token ())
        {
        }

        public ScmCombination (List<IExpression> exprList, Token token)
            : base (token)
        {
            m_Expressions = new List<IExpression> (exprList);
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[combination]";
        }
    }

    sealed class ScmSymbol : ScmAbstractExpression, IExpression
    {
        public ScmSymbol (Token token)
            : base (token)
        {
        }

        public ScmSymbol (String str)
            : base (new Token (str))
        {
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return String.Format ("#[symbol: {0}]", Token.AsString);
        }
    }

    sealed class ScmDotModifier : ScmAbstractExpression, IExpression
    {
        public ScmDotModifier (Token token)
            : base (token)
        {
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return String.Format ("#[modifier: .]");
        }
    }

    sealed class ScmString : ScmAbstractExpression, IExpression
    {
        public ScmString (Token token)
            : base (token)
        {
        }

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return String.Format ("#[string: {0}]", Token.AsString);
        }
    }

    abstract class AbstractNumber<T> : INumber where T : IFormattable
    {
        protected T m_Number;

        public T Value
        {
            get { return m_Number; }
        }

        public AbstractNumber (T num)
        {
            m_Number = num;
        }

        public abstract bool IsZero ();

        public string ToString (string format, IFormatProvider formatProvider)
        {
            return Value.ToString (format, formatProvider);
        }
    }

    sealed class ScmDoubleNumber : AbstractNumber<double>
    {
        public ScmDoubleNumber (double num)
            : base (num)
        {
        }

        public static ScmDoubleNumber operator + (ScmDoubleNumber n1, ScmDoubleNumber n2)
        {
            return new ScmDoubleNumber (n1.Value + n2.Value);
        }

        public static ScmDoubleNumber operator - (ScmDoubleNumber n1, ScmDoubleNumber n2)
        {
            return new ScmDoubleNumber (n1.Value - n2.Value);
        }

        public static ScmDoubleNumber operator / (ScmDoubleNumber n1, ScmDoubleNumber n2)
        {
            return new ScmDoubleNumber (n1.Value / n2.Value);
        }

        public static ScmDoubleNumber operator * (ScmDoubleNumber n1, ScmDoubleNumber n2)
        {
            return new ScmDoubleNumber (n1.Value * n2.Value);
        }

        public ScmDoubleNumber Abs ()
        {
            return new ScmDoubleNumber (Math.Abs (Value));
        }

        public override bool IsZero ()
        {
            return Value == 0;
        }
    }

    sealed class ScmIntegerNumber : AbstractNumber<BigInteger>
    {
        public ScmIntegerNumber (BigInteger num)
            : base (num)
        {
        }

        public static ScmIntegerNumber operator + (ScmIntegerNumber n1, ScmIntegerNumber n2)
        {
            return new ScmIntegerNumber (n1.Value + n2.Value);
        }

        public static ScmIntegerNumber operator - (ScmIntegerNumber n1, ScmIntegerNumber n2)
        {
            return new ScmIntegerNumber (n1.Value - n2.Value);
        }

        public static ScmIntegerNumber operator % (ScmIntegerNumber n1, ScmIntegerNumber n2)
        {
            return new ScmIntegerNumber (n1.Value % n2.Value);
        }

        public static ScmIntegerNumber operator / (ScmIntegerNumber n1, ScmIntegerNumber n2)
        {
            return new ScmIntegerNumber (BigInteger.Divide (n1.Value, n2.Value));
        }

        public static ScmIntegerNumber operator * (ScmIntegerNumber n1, ScmIntegerNumber n2)
        {
            return new ScmIntegerNumber (n1.Value * n2.Value);
        }

        public ScmIntegerNumber Abs ()
        {
            return new ScmIntegerNumber (BigInteger.Abs (Value));
        }

        public bool IsNegative ()
        {
            return Value.Sign < 0;
        }

        public bool IsPositive ()
        {
            return Value.Sign > 0;
        }

        public bool IsEven ()
        {
            return Value.IsEven;
        }

        public bool IsOdd ()
        {
            return !Value.IsEven;
        }

        public override bool IsZero ()
        {
            return Value.IsZero;
        }

        public static explicit operator ScmDoubleNumber (ScmIntegerNumber d)
        {
            return new ScmDoubleNumber ((double)d.Value);
        }
    }
}

