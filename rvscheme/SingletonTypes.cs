using System;

namespace rvscheme
{
    /// <summary>
    /// It's a generic singleton class that we will use 
    /// for expressions such as empty list, booleans etc.
    /// </summary>
    /// <typeparam name="T">We want T to be a class which has parameterless constructor.</typeparam>
    class Singleton<T> where T : class, new ()
    {
        // We don't care about when exactly these are initialized
        // so no specific constructors or something like that, just
        // plain and simple singleton.
        private static readonly T instance = new T ();
        public static T Instance
        {
            get { return instance; }
        }
    }

    sealed class ScmTrueValue : Singleton<ScmTrueValue>, IBoolean
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#t";
        }
    }

    sealed class ScmFalseValue : Singleton<ScmFalseValue>, IBoolean
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#f";
        }
    }

    sealed class ScmEmptyList : Singleton<ScmEmptyList>, IExpression
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "'()";
        }
    }

    sealed class ScmUnassigned : Singleton<ScmUnassigned>, IExpression
    {
        public string ToString (string format, IFormatProvider formatProvider)
        {
            return "#[Undefined]";
        }
    }
}

