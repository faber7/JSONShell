using System;
using System.Collections.Generic;
using System.Text;
using Shell.Generated;
using Shell.Interpreter;

namespace Shell.Types
{
    public class UserDefinedLambda : Lambda
    {
        private readonly ShellParser.FunctionContext context;
        private readonly ShellParser.StatementBlockContext statementBlock;

        /// <remark>
        /// function : KW_FUN LPAREN (functionArg (SYM_COMMA functionArg)*)? RPAREN statementBlock ;
        /// functionArg : typeSpecifier IDENTIFIER ;
        /// </remark>
        public UserDefinedLambda(ShellParser.FunctionContext ctx)
        {
            context = ctx;
            statementBlock = context.statementBlock();
            for (int i = 0; i < context.functionArg().Length; i++) {
                var arg = context.functionArg(i);
                string name = arg.IDENTIFIER().GetText();
                var token = Shell.Interpreter.Utility.GetTokenOfTypeSpecifier(arg.typeSpecifier());

                argList.Add(new Tuple<string, Specifier>(name, Shell.Interpreter.Utility.GetSpecifier(token)));
            }
        }

        public IShellReturnable Execute(ShellBaseVisitor<IShellReturnable> visitor)
        {
            return visitor.VisitStatementBlock(statementBlock);
        }

        override public int Arity() => argList.Count;
    }

    public abstract class BuiltinLambda : Lambda
    {
        public abstract IShellReturnable Execute(State state,List<Tuple<int, string, IShellData>> args);
        
        override public int Arity() => argList.Count;
    }
}