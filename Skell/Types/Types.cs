using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace Skell.Types
{
    // Base type for all internal datatypes
    public interface ISkellInternal
    {
    }

    // Base type for all types allowed in namespaces
    public interface ISkellNamedType : ISkellInternal
    {
    }

    // Represents all usable datatypes + None type
    public interface ISkellReturnable : ISkellInternal
    {
    }

    // Represents all usable datatypes in Skell
    public interface ISkellType : ISkellReturnable, ISkellNamedType
    {
    }

    // Represents all datatypes that have support for member access
    public interface ISkellIndexableType : ISkellType
    {
        int Count();

        ISkellType ThrowIndexOutOfRange(ISkellType index);

        bool Exists(ISkellType index);
        ISkellType GetMember(ISkellType index);
        void Replace(ISkellType index, ISkellType value);
        void Insert(ISkellType index, ISkellType value);
        void Delete(ISkellType index);
        ISkellReturnable IndexOf(ISkellType value);
    }

    /// <summary>
    /// Represents executable lambdas(both user-defined and builtin)
    /// </summary>
    public abstract class Lambda
    {
        public readonly List<Tuple<string, IToken>> argList = new List<Tuple<string, IToken>>();
        public string argString;
        
        /// <summary>
        /// Returns the number of arguments expected by this lambda
        /// </summary>
        public abstract int Arity();

        /// <summary>
        /// Names the arguments according to the lambda's arguments
        /// </summary>
        public List<Tuple<int, string, ISkellType>> NameArguments(
            List<Tuple<int, ISkellType>> args
        ) {
            if (args.Count != argList.Count) {
                return null;
            }

            var result = new List<Tuple<int, string, ISkellType>>();
            for (int i = 0; i < args.Count; i++) {
                result.Add(new Tuple<int, string, ISkellType>(
                    args[i].Item1,
                    argList[i].Item1,
                    args[i].Item2
                ));
            }

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
            for (int i = argList.Count; i < args.Count; i++) {
                extra.Add(new Tuple<int, Types.ISkellType>(i, args[i].Item3));
            }
            return extra;
        }

        /// <summary>
        /// Retrieves the arguments that do not have the same type.
        /// Use with GetExtraArgs().
        /// </summary>
        public List<Tuple<int, string, Antlr4.Runtime.IToken>> GetInvalidArgs(
            List<Tuple<int, string, Skell.Types.ISkellType>> args
        )
        {
            var ret = new List<Tuple<int, string, Antlr4.Runtime.IToken>>();
            int i = 0;
            foreach (var arg in argList) {
                if (i < args.Count && !Skell.Interpreter.Utility.MatchType(args[i].Item3, arg.Item2)) {
                    ret.Add(new Tuple<int, string, Antlr4.Runtime.IToken>(i, arg.Item1, arg.Item2));
                }
                i++;
            }
            return ret;
        }
    }
}