using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace Skell.Types
{
    // Base type for all datatypes in Skell
    public interface ISkellType
    {
    }

    public abstract class ISkellLambda : ISkellType
    {
        public readonly List<Tuple<string, IToken>> argsList = new List<Tuple<string, IToken>>();
        public string name;

        public abstract ISkellType execute(Skell.Generated.SkellBaseVisitor<ISkellType> visitor);
    }

    // Base type for any datatype that has member access support
    public interface ISkellIndexableType : ISkellType
    {
        int Count();

        ISkellType GetMember(ISkellType index);
    }
}