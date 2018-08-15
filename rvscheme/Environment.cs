using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    /// <summary>
    /// This is an environment frame. Simply put a table of bindings. These 
    /// frames are put together on a stack and form the whole run-time environment. 
    /// </summary>
    sealed class EnvironmentFrame
    {
        Dictionary<string, IExpression> m_Frame = new Dictionary<string, IExpression> ();

        /// <summary>
        /// Check whether the given symbol is present in this frame.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public bool Contains (ScmSymbol sym)
        {
            return m_Frame.ContainsKey (sym.Token.AsString);
        }

        /// <summary>
        /// Return a number of bindings in a frame.
        /// </summary>
        public int Size
        {
            get { return m_Frame.Count; }
        }

        /// <summary>
        /// Returns bound expression for the given symbol.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public IExpression Get (ScmSymbol sym)
        {
            if (Contains (sym))
                return m_Frame[sym.Token.AsString];
            return null;
        }

        /// <summary>
        /// Binds a symbol with an expression.
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="value"></param>
        public void Set (ScmSymbol sym, IExpression value)
        {
            if (!Contains (sym))
                m_Frame.Add (sym.Token.AsString, value);
            else
                m_Frame[sym.Token.AsString] = value;
        }
    }

    /// <summary>
    /// This class is just a stack of environment frames. It won't
    /// remove the first frame though -- it is global environment frame
    /// with predefined primitives.
    /// </summary>
    sealed class Environment
    {
        private Stack<EnvironmentFrame> m_Env = new Stack<EnvironmentFrame> ();

        /// <summary>
        /// Searches for a given symbol all the way down the stack; throws
        /// an exception if the symbol cannot be found.
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public IExpression Get (ScmSymbol sym)
        {
            foreach (var env in m_Env)
            {
                if (env.Contains (sym))
                    return env.Get (sym);
            }
            throw new EvaluatorException (sym.Token.AsString + " is unbound", sym);
        }

        /// <summary>
        /// Extends the environment by adding a given frame on top of the stack.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>This environment.</returns>
        public Environment Extend (EnvironmentFrame frame)
        {
            m_Env.Push (frame);
            return this;
        }

        /// <summary>
        /// Removes the top-most frame from the stack.
        /// </summary>
        public void Shrink ()
        {
            if (m_Env.Count > 1)
                m_Env.Pop ();
        }

        /// <summary>
        /// Peeks the top-most frame of the stack.
        /// </summary>
        public EnvironmentFrame CurrentFrame
        {
            get { return m_Env.Peek (); }
        }

        /// <summary>
        /// Creates a new binding on the top-most frame.
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="value"></param>
        public void Define (ScmSymbol sym, IExpression value)
        {
            CurrentFrame.Set (sym, value);
        }

        /// <summary>
        /// Updates the binding with the given expression. If the symbol
        /// cannot be found in the environment an exception is thrown.
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="value"></param>
        public void Set (ScmSymbol sym, IExpression value)
        {
            foreach (var env in m_Env)
            {
                if (env.Contains (sym))
                {
                    env.Set (sym, value);
                    return;
                }
            }
            throw new EvaluatorException (sym.Token.AsString + " is unbound", sym);
        }

        public Environment ()
        {
            // Set up global environment.
            var initialFrame = new EnvironmentFrame ();

            // Numeric operations.
            initialFrame.Set (new ScmSymbol ("+"), new ScmAddition ());
            initialFrame.Set (new ScmSymbol ("-"), new ScmSubtraction ());
            initialFrame.Set (new ScmSymbol ("*"), new ScmMultiplication ());
            initialFrame.Set (new ScmSymbol ("/"), new ScmDivision ());
            initialFrame.Set (new ScmSymbol ("zero?"), new ScmZeroPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("odd?"), new ScmOddPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("even?"), new ScmEvenPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("quotient"), new ScmQuotientProcedure ());
            initialFrame.Set (new ScmSymbol ("remainder"), new ScmRemainderProcedure ());
            initialFrame.Set (new ScmSymbol ("modulo"), new ScmModuloProcedure ());
            initialFrame.Set (new ScmSymbol ("abs"), new ScmAbsProcedure ());
            initialFrame.Set (new ScmSymbol ("min"), new ScmMinProcedure ());
            initialFrame.Set (new ScmSymbol ("max"), new ScmMaxProcedure ());
            initialFrame.Set (new ScmSymbol ("number?"), new ScmNumberPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("integer?"), new ScmIntegerPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("real?"), new ScmRealPredicateProcedure ());

            // TODO: exact?, inexact?, positive?, negative?,
            // sin, cos, sqrt etc.

            // True, false and comparison operations.
            initialFrame.Set (new ScmSymbol ("#t"), ScmTrueValue.Instance);
            initialFrame.Set (new ScmSymbol ("#f"), ScmFalseValue.Instance);
            initialFrame.Set (new ScmSymbol ("<"), new ScmLessThan ());
            initialFrame.Set (new ScmSymbol (">"), new ScmGreaterThan ());
            initialFrame.Set (new ScmSymbol ("="), new ScmEqualTo ());
            initialFrame.Set (new ScmSymbol ("<="), new ScmLessThanOrEqualTo ());
            initialFrame.Set (new ScmSymbol (">="), new ScmGreaterThanOrEqualTo ());

            // List procedures.
            initialFrame.Set (new ScmSymbol ("cons"), new ScmConsProcedure ());
            initialFrame.Set (new ScmSymbol ("list"), new ScmListProcedure ());
            initialFrame.Set (new ScmSymbol ("list?"), new ScmListPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("null?"), new ScmNullPredicateProcedure ());
            initialFrame.Set (new ScmSymbol ("length"), new ScmLengthProcedure ());
            initialFrame.Set (new ScmSymbol ("list-ref"), new ScmListRefProcedure ());
            initialFrame.Set (new ScmSymbol ("car"), new ScmCarProcedure ());
            initialFrame.Set (new ScmSymbol ("cdr"), new ScmCdrProcedure ());
            initialFrame.Set (new ScmSymbol ("set-car!"), new ScmSetCarProcedure ());
            initialFrame.Set (new ScmSymbol ("set-cdr!"), new ScmSetCdrProcedure ());

            // Continuation and evaluation procedures.
            initialFrame.Set (new ScmSymbol ("eval"), new ScmEvalProcedure ());
            initialFrame.Set (new ScmSymbol ("call/cc"), new ScmCallCcProcedure ());
            initialFrame.Set (new ScmSymbol ("call-with-current-continuation"), new ScmCallCcProcedure ());
            Extend (initialFrame);
        }

        /// <summary>
        /// Makes a copy of the given environment.
        /// </summary>
        /// <param name="e"></param>
        public Environment (Environment e)
        {
            m_Env = new Stack<EnvironmentFrame> (e.m_Env);
        }
    }
}

