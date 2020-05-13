using System;
using System.Collections.Generic;
using System.Text;

namespace Shell.Types
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
    public interface IShellInternal
    {
    }

    // Represents all named datatypes + None type, and is used by the visitor
    public interface IShellReturnable : IShellInternal
    {
    }

    // Base type for all values that can be named
    public interface IShellNamed : IShellReturnable
    {
    }

    // Represents all usable datatypes in Shell
    public interface IShellData : IShellReturnable, IShellNamed
    {
    }

    // Represents all datatypes that have support for member access
    public interface IShellIndexable : IShellData
    {
        Number Count();

        bool IsHomogeneous(Specifier type);
        IShellData[] ListIndices();
        IShellData[] ListValues();
        bool Exists(IShellData index);
        IShellData GetMember(IShellData index);
        void Replace(IShellData index, IShellData value);
        void Insert(IShellData index, IShellData value);
        void Delete(IShellData index);
        IShellReturnable IndexOf(IShellData value);
    }

    // Only for builtin library use, so appears as a namespaced identifier only
    public class Property : Shell.Types.IShellData
    {
        public Action<IShellData> action;
        private IShellData _value;

        public IShellData Value
        {
            get { return _value; }
            set
            {
                _value = value;
                action(_value);
            }
        }

        public Property(Action<IShellData> onChange)
        {
            action = onChange;
        }
    }

    /// <summary>
    /// Represents executable lambdas(both user-defined and builtin)
    /// </summary>
    public abstract class Lambda : IShellInternal
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
        public List<Tuple<int, string, IShellData>> NameArguments(
            List<Tuple<int, IShellData>> args
        )
        {
            if (args.Count != argList.Count)
                return null;

            var result = new List<Tuple<int, string, IShellData>>();
            for (int i = 0; i < args.Count; i++)
                result.Add(new Tuple<int, string, IShellData>(
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
        public List<Tuple<int, Shell.Types.IShellData>> GetExtraArgs(
            List<Tuple<int, string, Shell.Types.IShellData>> args
        )  
        {
            var extra = new List<Tuple<int, Shell.Types.IShellData>>();
            for (int i = argList.Count; i < args.Count; i++)
                extra.Add(new Tuple<int, Types.IShellData>(i, args[i].Item3));

            return extra;
        }

        /// <summary>
        /// Retrieves the arguments that do not have the same type.
        /// Use with GetExtraArgs().
        /// </summary>
        public List<Tuple<int, string, Shell.Types.Specifier>> GetInvalidArgs(
            List<Tuple<int, string, Shell.Types.IShellData>> args
        )
        {
            var ret = new List<Tuple<int, string, Specifier>>();
            int i = 0;
            foreach (var arg in argList) {
                if (i < args.Count && !Shell.Interpreter.Utility.MatchType(args[i].Item3, arg.Item2))
                    ret.Add(new Tuple<int, string, Specifier>(i, arg.Item1, arg.Item2));
                    
                i++;
            }
            return ret;
        }
    }
}