using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Skell.Generated;

namespace Skell.Types
{
    /// <summary>
    /// Represents a user-defined function in Skell
    /// </summary>
    /// <remark>
    /// function : LPAREN RPAREN statementBlock
    ///          | LPAREN (functionArg (SYM_COMMA functionArg)*)? statementBlock
    ///          ;
    /// functionArg : typeName IDENTIFIER ;
    /// </remark>
    public class Function : ISkellType
    {
        public readonly List<Tuple<string, IToken>> argsList;
        private readonly SkellParser.FunctionContext context;
        public readonly SkellParser.StatementBlockContext statementBlock;
        public readonly string name;

        public Function(string nm, SkellParser.FunctionContext ctx)
        {
            context = ctx;
            name = nm;
            statementBlock = context.statementBlock();
            argsList = new List<Tuple<string, IToken>>();
            for (int i = 0; i < context.functionArg().Length; i++) {
                var arg = context.functionArg(i);
                string name = Skell.Interpreter.Utility.GetIdentifierName(arg.IDENTIFIER());
                IToken token = Skell.Interpreter.Utility.GetTokenOfTypeName(arg.typeName());
                argsList.Add(new Tuple<string, IToken>(name, token));
            }
        }
    }
}