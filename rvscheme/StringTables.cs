using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rvscheme
{
    /// <summary>
    /// This class contains all the string messages used in this program.
    /// Strings should be added here and then referenced.
    /// </summary>
    static class UserMessages
    {
        public const string MalformedNumber = "Malformed number";
        public const string UnbalancedParanthesis = "Unbalanced paranthesis";
        public const string CannotApplyProcedure = "Cannot apply procedure";
        public const string SymbolIsUnassigned = "{0} is unassigned";
        public const string UnexpectedEndOfStream = "Unexpected end of stream";
        public const string SourceIsNotSet = "Source is not set";
        public const string ExpectingOneArgument = "Expecting 1 argument";
        public const string ExpectingTwoArguments = "Expecting 2 arguments";
        public const string ArgumentMustBeList = "Argument must be a list";
        public const string ArgumentMustBeInteger = "Argument must be an integer";
        public const string ArgumentMustBeProperList = "Argument must be a proper list";
        public const string IndexIsOutOfRange = "Index is out of range";
        public const string FirstArgMustBePair = "First argument must be a pair";
        public const string IllegalDatum = "Illegal datum";
        public const string CannotDivideByZero = "Cannot divide by zero";
        public const string MustContainOneExpressionAsArgument = "Must contain one expression as argument";
        public const string MustContainAtLeastOneClause = "Must contain at least one clause";
        public const string MustContainAtLeastOneExpression = "Must contain at least one expression as argument";
        public const string ExpectingOneClause = "Expects a clause";
        public const string MalformedIf = "Malformed if";
        public const string MalformedLet = "Malformed let";
        public const string MalformedLetrec = "Malformed let-rec";
        public const string MalformedLambda = "Malformed lambda";
        public const string MalformedDefine = "Malformed define";
        public const string MalformedSet = "Malformed set!";
        public const string ExpectingBindingList = "Expecting a binging list";
        public const string EmptyBindingList = "Empty binding list";
        public const string ExpectingPair = "Expecting a pair";
        public const string ExpectingName = "Expecting a name";
        public const string IllegalExpression = "Illegal expression";
        public const string LambdaMustHaveParamList = "Lambda must have parameters list (may be empty)";
        public const string MustBeIdentifier = "Must be an identifier";
        public const string MustHaveNameFormals = "Must have a name and zero or more formals";
        public const string MustHaveBody = "There must be at least one expression in the body";
        public const string ClauseCannotBeEmptyList = "Clause cannot be an empty list";
    }
}
