using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// Defines a new variable, that is binds a value to an identifier.
        /// </summary>
        private Bounce ApplyDefineVarSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var defineVarSpecialForm = expression as ScmDefineVarSpecialForm;
            // First evaluate the expression that we are going to bind to a name.
            return () => Eval (defineVarSpecialForm.Expression, env, evalResult =>
            {
                // Bind the value to the given name in the currect environment.
                env.Define (defineVarSpecialForm.Name, (IExpression)evalResult);
                // Define always returns the unassigned as result.
                return (Bounce)cont (ScmUnassigned.Instance);
            });
        }

        /// <summary>
        /// This method defines a procedure, effectively binds lambda to an identifier.
        /// </summary>
        private Bounce ApplyDefineProcSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            var defineProcSpecialForm = expression as ScmDefineProcSpecialForm;
            // Create a closure.
            var newProcedure = new ScmClosure (defineProcSpecialForm, env);
            // Bind it to the given name.
            env.Define (defineProcSpecialForm.ProcedureName, newProcedure);
            // Define returns the unassigned as result.
            return (Bounce)cont (ScmUnassigned.Instance);
        }
    }
}
