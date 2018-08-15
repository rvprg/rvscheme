using System;
namespace rvscheme
{
    abstract class AbstractException : Exception
    {
        private string m_Msg = String.Empty;
        private IExpression m_Expr = null;

        public override string Message
        {
            get
            {
                string eMsg = m_Msg;
                if (Data.Contains ("line") && Data.Contains ("column") && Data.Contains ("id"))
                {
                    eMsg = String.Format ("{0} ({1}, {2}): " + eMsg, Data["id"], Data["line"], Data["column"]);
                }
                return eMsg;
            }
        }

        public AbstractException (string msg, int l, int c, string sourceId)
        {
            m_Msg = msg;
            m_Expr = null;
        }

        public AbstractException (string msg, IExpression e)
        {
            m_Msg = msg;
            m_Expr = e;
            if (m_Expr is ScmAbstractExpression)
            {
                var exprComb = (m_Expr as ScmAbstractExpression);
                Data.Add("column", exprComb.Token.Column);
                Data.Add("line", exprComb.Token.Line);
                Data.Add("id",  exprComb.Token.Id);
            }
        }
    }

    sealed class ParserException : AbstractException
    {
        public ParserException (string msg, int l, int c, string sourceId)
            : base (msg, l, c, sourceId)
        {
        }
        public ParserException (string msg, IExpression e)
            : base (msg, e)
        {
        }
    }

    sealed class EvaluatorException : AbstractException
    {
        public EvaluatorException (string msg, IExpression e)
            : base (msg, e)
        {
        }
    }
}

