using System;
using System.Collections.Generic;
using System.Text;
using Skell.Generated;
using Skell.Interpreter;

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
            for (int i = 0; i < context.functionArg().Length; i++) {
                var arg = context.functionArg(i);
                string name = arg.IDENTIFIER().GetText();
                var token = Skell.Interpreter.Utility.GetTokenOfTypeSpecifier(arg.typeSpecifier());

                argList.Add(new Tuple<string, Specifier>(name, Skell.Interpreter.Utility.GetSpecifier(token)));
            }
        }

        public ISkellReturnable Execute(SkellBaseVisitor<ISkellReturnable> visitor)
        {
            return visitor.VisitStatementBlock(statementBlock);
        }

        override public int Arity() => argList.Count;
    }

    public abstract class BuiltinLambda : Lambda
    {
        public abstract ISkellReturnable Execute(State state,List<Tuple<int, string, ISkellType>> args);
        
        override public int Arity() => argList.Count;
    }
}