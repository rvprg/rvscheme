using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// This is a special class - continuation. We define here, since that 
        /// is something that belongs to the evaluator. It simply encapsulates a 
        /// continuation object.
        /// </summary>
        sealed class ScmContinuation : IProcedure
        {
            Cont m_Cont = null;

            public Cont Cont
            {
                get { return m_Cont; }
            }

            public ScmContinuation (Cont cont)
            {
                m_Cont = cont;
            }

            public string ToString (string format, IFormatProvider formatProvider)
            {
                return "#[continuation]";
            }
        }
    }
}
