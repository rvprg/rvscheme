using System;
namespace rvscheme
{
    /// <summary>
    /// Every object we operate is an expression. Any expression is formattable.
    /// </summary>
    interface IExpression : IFormattable
    {
    }

    /// <summary>
    /// Any procedure is an expression itself.
    /// </summary>
    interface IProcedure : IExpression
    {
    }

    /// <summary>
    /// Any primitive procedure (those that are built-in to the 
    /// language) is a procedure.
    /// </summary>
    interface IPrimitiveProcedure : IProcedure
    {
    }

    /// <summary>
    /// Arithmetic operation is a primitive procedure.
    /// </summary>
    interface INumericalOperation : IPrimitiveProcedure
    {
    }

    /// <summary>
    /// Comparison operation is a primitive procedure.
    /// </summary>
    interface IComparisonOperation : IPrimitiveProcedure
    {
    }

    /// <summary>
    /// Any special form is an expression.
    /// </summary>
    interface ISpecialForm : IExpression
    {
    }

    /// <summary>
    /// Boolean value is an expression.
    /// </summary>
    interface IBoolean : IExpression
    {
    }

    /// <summary>
    /// Numeric value is an expression.
    /// </summary>
    interface INumber : IExpression
    {
    }

    interface INumericalOperations
    {
    }

    interface IComparisonOperations
    {
    }
}

