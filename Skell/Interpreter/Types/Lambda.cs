using System.Linq;
using System.Collections.Generic;
using Antlr4.Runtime;
using Generated;

namespace Skell.Types
{
    /// <summary>
    /// Represents a lambda(anonymous function) in Skell
    /// </summary>
    /// <remark>
    /// lambda : LPAREN RPAREN statementBlock
    ///        | LPAREN lambdaArg (SYM_COMMA lambdaArg)* statementBlock
    ///        ;
    /// lambdaArg : typeName IDENTIFIER ;
    /// </remark>
    class Lambda : ISkellType
    {
        public readonly Dictionary<string, IToken> argsList;
        private readonly SkellParser.LambdaContext context;
        public readonly SkellParser.StatementBlockContext statementBlock;

        public Lambda(SkellParser.LambdaContext ctx)
        {
            context = ctx;
            statementBlock = context.statementBlock();
            argsList = new Dictionary<string, IToken>();
            for (int i = 0; i < context.lambdaArg().Length; i++) {
                var arg = context.lambdaArg(i);
                string name = Skell.Interpreter.Utility.GetIdentifierName(arg.IDENTIFIER());
                IToken token = Skell.Interpreter.Utility.GetTokenOfTypeName(arg.typeName());
                argsList.Add(name, token);
            }
        }
    }
}