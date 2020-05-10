using System;
using System.Collections.Generic;
using System.Text;

namespace Skell.Types
{
    public enum Specifier
    {
        Any,
        Bool,
        Number,
        String,
        Object,
        Array
    }

    // Base type for all internal datatypes
    public interface ISkellInternal
    {
    }

    // Represents all named datatypes + None type, and is used by the visitor
    public interface ISkellReturnable : ISkellInternal
    {
    }

    // Base type for all values that can be named
    public interface ISkellNamedType : ISkellReturnable
    {
    }

    // Represents all usable datatypes in Skell
    public interface ISkellType : ISkellReturnable, ISkellNamedType
    {
    }

    // Represents all datatypes that have support for member access
    public interface ISkellIndexable : ISkellType
    {
        Number Count();

        bool IsHomogeneous(Specifier type);
        ISkellType[] ListIndices();
        ISkellType[] ListValues();
        bool Exists(ISkellType index);
        ISkellType GetMember(ISkellType index);
        void Replace(ISkellType index, ISkellType value);
        void Insert(ISkellType index, ISkellType value);
        void Delete(ISkellType index);
        ISkellReturnable IndexOf(ISkellType value);
    }

    // Only for builtin library use, so appears as a namespaced identifier only
    public class Property : Skell.Types.ISkellType
    {
        public Action<ISkellType> action;
        private ISkellType _value;

        public ISkellType value
        {
            get { return _value; }
            set
            {
                _value = value;
                action(_value);
            }
        }

        public Property(Action<ISkellType> onChange)
        {
            action = onChange;
        }
    }

    /// <summary>
    /// Represents executable lambdas(both user-defined and builtin)
    /// </summary>
    public abstract class Lambda : ISkellInternal
    {
        public List<Tuple<string, Specifier>> argList = new List<Tuple<string, Specifier>>();
        
        /// <summary>
        /// Returns the number of arguments expected by this lambda
        /// </summary>
        public abstract int Arity();

        override public string ToString()
        {
            var repr = new StringBuilder("(");
            for (int i = 0; i < argList.Count; i++)
            {
                var arg = argList[i];
                repr.Append($"{arg.Item2} {arg.Item1}");
                if (i + 1 != argList.Count)
                    repr.Append(", ");
            }
            repr.Append(")");
            return repr.ToString();
        }

        /// <summary>
        /// Names the arguments according to the lambda's arguments
        /// </summary>
        public List<Tuple<int, string, ISkellType>> NameArguments(
            List<Tuple<int, ISkellType>> args
        )
        {
            if (args.Count != argList.Count)
                return null;

            var result = new List<Tuple<int, string, ISkellType>>();
            for (int i = 0; i < args.Count; i++)
                result.Add(new Tuple<int, string, ISkellType>(
                    args[i].Item1,
                    argList[i].Item1,
                    args[i].Item2
                ));

            return result;
        }

        /// <summary>
        /// Retrieves the extra arguments that are to be passed to the function.
        /// Use with GetInvalidArgs().
        /// </summary>
        public List<Tuple<int, Skell.Types.ISkellType>> GetExtraArgs(
            List<Tuple<int, string, Skell.Types.ISkellType>> args
        )  
        {
            var extra = new List<Tuple<int, Skell.Types.ISkellType>>();
            for (int i = argList.Count; i < args.Count; i++)
                extra.Add(new Tuple<int, Types.ISkellType>(i, args[i].Item3));

            return extra;
        }

        /// <summary>
        /// Retrieves the arguments that do not have the same type.
        /// Use with GetExtraArgs().
        /// </summary>
        public List<Tuple<int, string, Skell.Types.Specifier>> GetInvalidArgs(
            List<Tuple<int, string, Skell.Types.ISkellType>> args
        )
        {
            var ret = new List<Tuple<int, string, Specifier>>();
            int i = 0;
            foreach (var arg in argList) {
                if (i < args.Count && !Skell.Interpreter.Utility.MatchType(args[i].Item3, arg.Item2))
                    ret.Add(new Tuple<int, string, Specifier>(i, arg.Item1, arg.Item2));
                    
                i++;
            }
            return ret;
        }
    }
}