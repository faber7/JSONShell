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
        private readonly ShellParser.Statement_blockContext statement_block;

        /// <remark>
        /// function : KW_FUN LPAREN (function_argument (SYM_COMMA function_argument)*)? RPAREN statement_block ;
        /// function_argument : argument_type_specifier IDENTIFIER ;
        /// </remark>
        public UserDefinedLambda(ShellParser.FunctionContext ctx)
        {
            context = ctx;
            statement_block = context.statement_block();
            for (int i = 0; i < context.function_argument().Length; i++) {
                var arg = context.function_argument(i);
                string name = arg.IDENTIFIER().GetText();
                var token = Shell.Interpreter.Utility.GetTokenOfArgumentTypeSpecifier(arg.argument_type_specifier());

                argList.Add(new Tuple<string, Specifier>(name, Shell.Interpreter.Utility.GetSpecifier(token)));
            }
        }

        public IShellReturnable Execute(ShellBaseVisitor<IShellReturnable> visitor)
        {
            return visitor.VisitStatement_block(statement_block);
        }

        override public int Arity() => argList.Count;
    }

    public abstract class BuiltinLambda : Lambda
    {
        public abstract IShellReturnable Execute(State state,List<Tuple<int, string, IShellData>> args);
        
        override public int Arity() => argList.Count;
    }
}