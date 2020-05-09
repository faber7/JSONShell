using System;
using System.Collections.Generic;
using System.Text;
using Skell.Generated;

namespace Skell.Types
{
    public class UserDefinedLambda : Lambda
    {
        private readonly SkellParser.FunctionContext context;
        private readonly SkellParser.StatementBlockContext statementBlock;

        /// <remark>
        /// function : KW_FUN LPAREN (functionArg (SYM_COMMA functionArg)*)? RPAREN statementBlock ;
        /// functionArg : typeSpecifier IDENTIFIER ;
        /// </remark>
        public UserDefinedLambda(SkellParser.FunctionContext ctx)
        {
            context = ctx;
            statementBlock = context.statementBlock();
            var repr = new StringBuilder("(");
            for (int i = 0; i < context.functionArg().Length; i++) {
                var arg = context.functionArg(i);
                string name = arg.IDENTIFIER().GetText();
                var token = Skell.Interpreter.Utility.GetTokenOfTypeSpecifier(arg.typeSpecifier());

                argList.Add(new Tuple<string, Specifier>(name, Skell.Interpreter.Utility.GetSpecifier(token)));

                repr.Append($"{token.Text} {name}");
                if (i + 1 != context.functionArg().Length)
                    repr.Append(", ");
            }
            repr.Append(")");
            argString = repr.ToString();
        }

        public ISkellReturnable Execute(SkellBaseVisitor<ISkellReturnable> visitor)
        {
            return visitor.VisitStatementBlock(statementBlock);
        }

        override public string ToString()
        {
            return argString;
        }

        override public int Arity() => argList.Count;
    }

    public abstract class BuiltinLambda : Lambda
    {
        public abstract ISkellReturnable Execute(List<Tuple<int, string, ISkellType>> args);

        override public string ToString()
        {
            return argString;
        }
        
        override public int Arity() => argList.Count;
    }
}