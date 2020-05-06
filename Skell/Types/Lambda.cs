using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Skell.Generated;

namespace Skell.Types
{
    public class UserDefinedLambda : Lambda
    {
        private readonly SkellParser.FunctionContext context;
        private readonly SkellParser.StatementBlockContext statementBlock;

        /// <remark>
        /// function : KW_FUN LPAREN (functionArg (SYM_COMMA functionArg)*)? RPAREN statementBlock ;
        /// functionArg : typeName IDENTIFIER ;
        /// </remark>
        public UserDefinedLambda(
            SkellParser.FunctionContext ctx
        ) {
            context = ctx;
            statementBlock = context.statementBlock();
            var repr = new StringBuilder("(");
            for (int i = 0; i < context.functionArg().Length; i++) {
                var arg = context.functionArg(i);
                string name = Skell.Interpreter.Utility.GetIdentifierName(arg.IDENTIFIER());
                IToken token = Skell.Interpreter.Utility.GetTokenOfTypeName(arg.typeName());
                argList.Add(new Tuple<string, IToken>(name, token));

                repr.Append($"{token.Text} {name}");
                if (i + 1 != context.functionArg().Length) {
                    repr.Append(", ");
                }
            }
            repr.Append(")");
            argString = repr.ToString();
        }

        public ISkellType Execute(SkellBaseVisitor<ISkellType> visitor)
        {
            return visitor.VisitStatementBlock(statementBlock);
        }

        override public string ToString()
        {
            return argString;
        }

        override public int Arity() => argList.Count;
    }

    public class BuiltinLambda : Lambda
    {
        public Func<ISkellType> Execute;

        public BuiltinLambda(
            List<Tuple<string, IToken>> args,
            Func<ISkellType> fn
        ) {
            var repr = new StringBuilder("(");
            for (int i = 0; i < args.Count; i++) {
                var pair = args[i];
                argList.Add(pair);

                repr.Append($"{pair.Item2.Text} {pair.Item1}");
                if (i + 1 != args.Count) {
                    repr.Append(", ");
                }
            }
            repr.Append(")");

            argString = repr.ToString();
            Execute = fn;
        }

        override public string ToString()
        {
            return argString;
        }
        
        override public int Arity() => argList.Count;
    }
}