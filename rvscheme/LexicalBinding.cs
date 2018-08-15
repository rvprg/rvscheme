using System;
using System.Collections.Generic;
using System.Text;

namespace rvscheme
{
    sealed partial class Evaluator
    {
        /// <summary>
        /// This method binds names with values allowing their usage within expressions
        /// that are being bound.
        /// </summary>
        private Bounce ApplyLetrecSpecialForm (IExpression expression, Environment env, Cont cont)
        {
            // In comments below we refer to bindings as key-value pairs.
            var letRecSpecialForm = expression as ScmLetrecSpecialForm;
            // We are going to use a temporary environmental frame, using which we are 
            // going to define dummy values for each identifier first, and then substitute with
            // evaluated ones.
            var environmentFrame = new EnvironmentFrame ();
            // A list of names (keys) that we are going to bind.
            var bindingKeys = new List<IExpression> ();
            // A list of corresponding values.
            var bindingValues = new List<IExpression> ();
            // Proccess each pair in the list of bindings, by saving
            // them in corresponding lists and assigning
            // the unassigned value to each of the identifiers.
            foreach (var keyValue in letRecSpecialForm.Bindings)
            {
                bindingValues.Add (keyValue.Value);
                bindingKeys.Add (keyValue.Key);
                environmentFrame.Set ((ScmSymbol)keyValue.Key, ScmUnassigned.Instance);
            }
            // Now evaluate each expression (i.e. "value").
            return () => EvalList (bindingValues, env, evalBindingsResult =>
            {
                var bindingResultsAsList = evalBindingsResult as List<IExpression>;
                // Substitute evaluated results.
                for (int i = 0; i < bindingKeys.Count; ++i)
                {
                    var bindingName = bindingKeys[i] as ScmSymbol;
                    var bindingValue = bindingResultsAsList[i];
                    environmentFrame.Set (bindingName, bindingValue);
                }
                // Extend the environment with this new frame.
                // FIXME: it should "branch" here, and not create a copy of entire environment.
                var newEnvironment = (new Environment (env)).Extend (environmentFrame);
                // And evalute expressions in the environment with those bindings.
                return EvalList (letRecSpecialForm.Expressions, newEnvironment, evalResults =>
                {
                    // FIXME: rewrite this in the same manner as for closure.
                    var evalResultsAsList = evalResults as List<IExpression>;
                    return (Bounce)cont (evalResultsAsList.Last ());
                });
            });
        }

        /// <summary>
        /// This method binds names with values, but doesn't allow their usage within expressions
        /// that are being bound.
        /// </summary>
        private Bounce ApplyLetSpecialForm (IExpression expressions, Environment env, Cont cont)
        {
            var letSpecialForm = expressions as ScmLetSpecialForm;
            // Split keys and values in separate lists.
            var bindingKeys = new List<IExpression> ();
            var bindingValues = new List<IExpression> ();
            foreach (var keyValue in letSpecialForm.Bindings)
            {
                bindingValues.Add (keyValue.Value);
                bindingKeys.Add (keyValue.Key);
            }
            // Evaluate values.
            return () => EvalList (bindingValues, env, evalBindingResults =>
            {
                var bindingResultsAsList = evalBindingResults as List<IExpression>;
                // Create a temporary environment frame that will hold these
                // bindings.
                var environmentFrame = new EnvironmentFrame ();
                for (int i = 0; i < bindingKeys.Count; ++i)
                {
                    var bindingName = bindingKeys[i] as ScmSymbol;
                    var bindingValue = bindingResultsAsList[i];
                    environmentFrame.Set (bindingName, bindingValue);
                }
                // Extend the environemnt with these bindings.
                var newEnvironment = (new Environment (env)).Extend (environmentFrame);
                // Evaluate the body in the new environment.
                return EvalList (letSpecialForm.Expressions, newEnvironment, evalResults =>
                {
                    var evalResultsAsList = evalResults as List<IExpression>;
                    return (Bounce)cont (evalResultsAsList.Last ());
                });
            });
        }
    }
}
