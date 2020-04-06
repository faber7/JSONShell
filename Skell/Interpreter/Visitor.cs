using Generated;
using Serilog;

namespace Skell.Interpreter
{
    internal class SkellVisitor : SkellBaseVisitor<Skell.Types.ISkellType>
    {
        private readonly Skell.Types.Boolean defaultReturnValue = new Skell.Types.Boolean(true);
        private static ILogger logger;
        private static Context globalContext;
        private Context currentContext;

        public SkellVisitor()
        {
            logger = Log.ForContext<SkellVisitor>();
            globalContext = new Context();
            currentContext = globalContext;
        }

        /// <summary>
        /// program : statement+ ;
        /// </summary>
        override public Skell.Types.ISkellType VisitProgram(SkellParser.ProgramContext context)
        {
            Skell.Types.ISkellType lastResult = defaultReturnValue;
            foreach (var statement in context.statement()) {
                logger.Verbose("Visiting:\n" + statement.ToStringTree());
                lastResult = Visit(statement);
                if (lastResult is Skell.Types.Array arr) {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()} and length {arr.Count()}");
                } else if (lastResult is Skell.Types.Object obj) {
                    logger.Debug($"Result: \n{obj}\n of type {lastResult.GetType()}");
                } else {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()}");
                }
                System.Console.WriteLine(lastResult);
            }
            return lastResult;
        }

        /// <summary>
        /// statement : declaration EOL | expression EOL | control ;
        /// </summary>
        override public Skell.Types.ISkellType VisitStatement(SkellParser.StatementContext context)
        {
            if (context.expression() != null) {
                return VisitExpression(context.expression());
            } else if (context.control() != null) {
                return VisitControl(context.control());
            } else {
                return VisitDeclaration(context.declaration());
            }
        }

        /// <summary>
        /// statementBlock : LCURL statement* RCURL ;
        /// </summary>
        override public Skell.Types.ISkellType VisitStatementBlock(SkellParser.StatementBlockContext context)
        {
            Skell.Types.ISkellType lastResult = defaultReturnValue;
            for (int i = 0; i < context.statement().Length; i++) {
                lastResult = VisitStatement(context.statement(i));
            }
            return lastResult;
        }

        /// <summary>
        /// declaration : varDecl ;
        /// </summary>
        override public Skell.Types.ISkellType VisitDeclaration(SkellParser.DeclarationContext context)
        {
            return VisitVarDecl(context.varDecl());
        }

        /// <summary>
        /// varDecl : typeName IDENTIFIER
        ///         | typeName IDENTIFIER OP_ASSGN expression;
        /// </summary>
        override public Skell.Types.ISkellType VisitVarDecl(SkellParser.VarDeclContext context)
        {
            var typeToken = Utility.GetTypeNameToken(context.typeName());
            Skell.Types.ISkellType data = null;
            switch(typeToken.Type) {
                case SkellLexer.TYPE_OBJECT:
                    data = new Skell.Types.Object();
                break;
                case SkellLexer.TYPE_ARRAY:
                    data = new Skell.Types.Array();
                break;
                case SkellLexer.TYPE_NUMBER:
                    data = new Skell.Types.Number();
                break;
                case SkellLexer.TYPE_STRING:
                    data = new Skell.Types.String();
                break;
                case SkellLexer.TYPE_BOOL:
                    data = new Skell.Types.Boolean();
                break;
                default:
                // given type is null
                throw new System.NotImplementedException();
            }
            if (context.expression() != null) {
                var exp = VisitExpression(context.expression());
                if (exp.GetType() != data.GetType()) {
                    throw new System.NotImplementedException();
                }
                data = exp;
            }

            string name = Utility.GetIdentifierName(context.IDENTIFIER());

            currentContext.Set(name, data);

            return defaultReturnValue;
        }

        /// <summary>
        /// expression : eqExpr ;
        /// </summary>
        override public Skell.Types.ISkellType VisitExpression(SkellParser.ExpressionContext context)
        {
            return Visit(context.eqExpr());
        }

        /// <summary>
        /// eqExpr : relExpr ((OP_NE | OP_EQ) relExpr)? ;
        /// </summary>
        override public Skell.Types.ISkellType VisitEqExpr(SkellParser.EqExprContext context)
        {
            Skell.Types.ISkellType result = VisitRelExpr(context.relExpr(0));

            if (context.relExpr(1) != null) {
                Skell.Types.ISkellType next = VisitRelExpr(context.relExpr(1));
                var op = (Antlr4.Runtime.IToken) context.relExpr(1).GetLeftSibling().Payload;
                bool ret = result.Equals(next);
                if (op.Type == SkellLexer.OP_NE) {
                    ret = !ret;
                }
                return new Skell.Types.Boolean(ret);
            }
            return result;
        }

        /// <summary>
        /// relExpr : addExpr ((OP_GT | OP_GE | OP_LT | OP_LE) addExpr)? ;
        /// </summary>
        override public Skell.Types.ISkellType VisitRelExpr(SkellParser.RelExprContext context)
        {
            Skell.Types.ISkellType result = VisitAddExpr(context.addExpr(0));

            if (context.addExpr(1) != null) {
                Skell.Types.ISkellType next = VisitAddExpr(context.addExpr(1));
                var op = (Antlr4.Runtime.IToken) context.addExpr(1).GetLeftSibling().Payload;
                if (result is Skell.Types.Number r && next is Skell.Types.Number n) {
                    bool ret = false;
                    switch (op.Type) {
                        case SkellLexer.OP_GE:
                            ret = r >= n;
                        break;
                        case SkellLexer.OP_GT:
                            ret = r > n;
                        break;
                        case SkellLexer.OP_LE:
                            ret = r < n;
                        break;
                        case SkellLexer.OP_LT:
                            ret = r <= n;
                        break;
                    }
                    return new Skell.Types.Boolean(ret);
                }
            }
            return result;
        }

        /// <summary>
        /// addExpr : mulExpr ((OP_SUB | OP_ADD) mulExpr)* ;
        /// </summary>
        override public Skell.Types.ISkellType VisitAddExpr(SkellParser.AddExprContext context)
        {
            int i = 0;
            Skell.Types.ISkellType result = VisitMulExpr(context.mulExpr(i));

            i++;
            while (context.mulExpr(i) != null) {
                if (!(result is Skell.Types.Number)) {
                    throw new System.NotImplementedException();
                }
                Skell.Types.ISkellType next = VisitMulExpr(context.mulExpr(i));
                if (!(next is Skell.Types.Number)) {
                    throw new System.NotImplementedException();
                }
                var op = (Antlr4.Runtime.IToken) context.mulExpr(i).GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_SUB) {
                    result = (Skell.Types.Number) result - (Skell.Types.Number) next;
                } else {
                    result = (Skell.Types.Number) result + (Skell.Types.Number) next;
                }
                i++;
            }
            return result;
        }

        /// <summary>
        /// mulExpr : unary ((OP_DIV | OP_MUL) unary)* ;
        /// </summary>
        override public Skell.Types.ISkellType VisitMulExpr(SkellParser.MulExprContext context)
        {
            int i = 0;
            Skell.Types.ISkellType result = VisitUnary(context.unary(i));

            i++;
            while (context.unary(i) != null) {
                if (!(result is Skell.Types.Number)) {
                    throw new System.NotImplementedException();
                }
                Skell.Types.ISkellType next = VisitUnary(context.unary(i));
                if (!(next is Skell.Types.Number)) {
                    throw new System.NotImplementedException();
                }
                var op = (Antlr4.Runtime.IToken) context.unary(i).GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_DIV) {
                    result = (Skell.Types.Number) result / (Skell.Types.Number) next;
                } else {
                    result = (Skell.Types.Number) result * (Skell.Types.Number) next;
                }
                i++;
            }
            return result;
        }

        /// <summary>
        /// control : ifControl ;
        /// </summary>
        override public Skell.Types.ISkellType VisitControl(SkellParser.ControlContext context)
        {
            return VisitIfControl(context.ifControl());
        }

        /// <summary>
        /// ifControl : ifThenControl | ifThenElseControl ;
        /// </summary>
        override public Skell.Types.ISkellType VisitIfControl(SkellParser.IfControlContext context)
        {
            if (context.ifThenControl() != null) {
                return VisitIfThenControl(context.ifThenControl());
            } else {
                return VisitIfThenElseControl(context.ifThenElseControl());
            }
        }

        /// <summary>
        /// ifThenControl : KW_IF expression KW_THEN statementBlock ;
        /// </summary>
        override public Skell.Types.ISkellType VisitIfThenControl(SkellParser.IfThenControlContext context)
        {
            Skell.Types.Boolean cont = Utility.EvaluateExpr(this, context.expression());
            if (!cont.value) {
                return new Skell.Types.Boolean(false);
            }
            return VisitStatementBlock(context.statementBlock());
        }

        /// <summary>
        /// ifThenElseControl : ifThenControl KW_ELSE (statementBlock | ifControl) ;
        /// </summary>
        override public Skell.Types.ISkellType VisitIfThenElseControl(SkellParser.IfThenElseControlContext context)
        {
            // Evaluate the expression separately
            // Do not use VisitIfThenControl as the statementBlock in the
            // ifThenControl can return a false value
            Skell.Types.Boolean cont = Utility.EvaluateExpr(this, context.ifThenControl().expression());
            if (cont.value) {
                return VisitStatementBlock(context.ifThenControl().statementBlock());
            } else if (context.statementBlock() != null) {
                return VisitStatementBlock(context.statementBlock());
            } else {
                return VisitIfControl(context.ifControl());
            }
        }

        /// <summary>
        /// unary : (OP_NOT | OP_SUB) unary
        ///       | primary
        ///       ;
        /// </summary>
        override public Skell.Types.ISkellType VisitUnary(SkellParser.UnaryContext context)
        {
            if (context.unary() != null) {
                var op = (Antlr4.Runtime.IToken) context.unary().GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_NOT) {
                    Skell.Types.Boolean boolUnary = new Skell.Types.Boolean(VisitUnary(context.unary()).ToString());
                    return !boolUnary;
                }
                Skell.Types.Number numUnary = new Skell.Types.Number(VisitUnary(context.unary()).ToString());
                return -numUnary;
            }
            return VisitPrimary(context.primary());
        }

        /// <summary>
        /// primary : term
        ///         | LPAREN expression RPAREN
        ///         | primary LSQR (STRING | NUMBER | IDENTIFIER) RSQR
        ///         ;
        /// </summary>
        override public Skell.Types.ISkellType VisitPrimary(SkellParser.PrimaryContext context)
        {
            if (context.primary() != null) {
                Skell.Types.ISkellType term = VisitPrimary(context.primary());
                Skell.Types.ISkellType index;
                if (context.STRING() != null) {
                    index = Utility.GetString(context.STRING());
                } else if (context.NUMBER() != null) {
                    index = new Skell.Types.Number(context.NUMBER().GetText());
                } else {
                    index = currentContext.Get(Utility.GetIdentifierName(context.IDENTIFIER()));
                }
                if (term is Skell.Types.Object obj) {
                    return obj.GetMember(index);
                } else if (term is Skell.Types.Array arr) {
                    return arr.GetMember(index);
                }
                throw new System.NotImplementedException();
            } else if (context.term() != null) {
                return VisitTerm(context.term());
            }
            return VisitExpression(context.expression());
        }

        /// <summary>
        /// term : value
        ///      | IDENTIFIER
        ///      ;
        /// </summary>
        override public Skell.Types.ISkellType VisitTerm(SkellParser.TermContext context)
        {
            if (context.IDENTIFIER() != null) {
                return currentContext.Get(Utility.GetIdentifierName(context.IDENTIFIER()));
            }
            return VisitValue(context.value());
        }

        /// <summary>
        /// value : object | array | STRING | NUMBER | bool ;
        /// </summary>
        override public Skell.Types.ISkellType VisitValue(SkellParser.ValueContext context)
        {
            if (context.@object() != null) {
                return VisitObject(context.@object());
            }
            if (context.array() != null) {
                return VisitArray(context.array());
            }
            if (context.STRING() != null) {
                return Utility.GetString(context.STRING());
            }
            if (context.NUMBER() != null) {
                return new Skell.Types.Number(context.NUMBER().GetText());
            }
            return new Skell.Types.Boolean(context.@bool().GetText());
        }

        /// <summary>
        /// array : LSQR value (SYM_COMMA value)* RSQR
        ///       | LSQR RSQR
        ///       ;
        /// </summary>
        override public Skell.Types.ISkellType VisitArray(SkellParser.ArrayContext context)
        {
            SkellParser.ValueContext[] values = context.value();
            Skell.Types.ISkellType[] contents = new Skell.Types.ISkellType[values.Length];
            for (int i = 0; i < values.Length; i++) {
                contents[i] = VisitValue(values[i]);
            }
            return new Skell.Types.Array(contents);
        }

        /// <summary>
        /// object : LCURL pair (SYM_COMMA pair)* RCURL
        ///        | LCURL RCURL
        ///        ;
        /// pair : STRING SYM_COLON value ;
        /// </summary>
        override public Skell.Types.ISkellType VisitObject(SkellParser.ObjectContext context)
        {
            Skell.Types.String[] keys = new Skell.Types.String[context.pair().Length];
            Skell.Types.ISkellType[] values = new Skell.Types.ISkellType[context.pair().Length];

            for (int i = 0; i < context.pair().Length; i++) {
                var pair = context.pair(i);
                keys[i] = Utility.GetString(pair.STRING());
                values[i] = VisitValue(pair.value());
            }

            return new Skell.Types.Object(keys, values);
        }
    }

    internal static class Utility
    {
        /// <summary>
        /// Returns a Skell.Types.String for a STRING token
        /// </summary>
        /// <remark>
        /// STRING : SYM_QUOTE (ESC | SAFECODEPOINT)* SYM_QUOTE ;
        /// </remark>
        public static Skell.Types.String GetString(Antlr4.Runtime.Tree.ITerminalNode context)
        {
            string contents = context.GetText();
            contents = contents.Remove(0,1);
            contents = contents.Remove(contents.Length - 1,1);
            return new Skell.Types.String(contents);
        }

        /// <summary>
        /// Returns the token for the typename
        /// </summary>
        /// <remark>
        /// typeName : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL | TYPE_NULL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTypeNameToken(SkellParser.TypeNameContext context)
        {
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }

        /// <summary>
        /// Returns the name of the identifier
        /// </summary>
        /// <remark>
        /// IDENTIFIER: NONDIGIT (NONDIGIT | DIGIT)* ;
        /// </remark>
        public static string GetIdentifierName(Antlr4.Runtime.Tree.ITerminalNode context)
        {
            return context.GetText();
        }

        /// <summary>
        /// Evaluates an expression and returns a Skell.Types.Boolean
        /// </summary>
        public static Skell.Types.Boolean EvaluateExpr(SkellVisitor parser, SkellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            if (!(expressionResult is Skell.Types.Boolean)) {
                throw new System.NotImplementedException();
            }
            return (Skell.Types.Boolean) expressionResult;
        }

        /// <summary>
        /// Returns the left sibling of the parse tree node.
        /// </summary>
        /// <param name="context">A node.</param>
        /// <returns>Left sibling of a node, or null if no sibling is found.</returns>
        public static Antlr4.Runtime.Tree.IParseTree GetLeftSibling(this Antlr4.Runtime.ParserRuleContext context)
        {
            int index = GetNodeIndex(context);

            return index > 0 && index < context.Parent.ChildCount
                ? context.Parent.GetChild(index - 1)
                : null;
        }

        /// <summary>
        /// Returns the right sibling of the parse tree node.
        /// </summary>
        /// <param name="context">A node.</param>
        /// <returns>Right sibling of a node, or null if no sibling is found.</returns>
        public static Antlr4.Runtime.Tree.IParseTree GetRightSibling(this Antlr4.Runtime.ParserRuleContext context)
        {
            int index = GetNodeIndex(context);

            return index >= 0 && index < context.Parent.ChildCount - 1
                ? context.Parent.GetChild(index + 1)
                : null;
        }

        /// <summary>
        /// Returns the node's index with in its parent's children array.
        /// </summary>
        /// <param name="context">A child node.</param>
        /// <returns>Node's index or -1 if node is null or doesn't have a parent.</returns>
        public static int GetNodeIndex(this Antlr4.Runtime.ParserRuleContext context)
        {
            Antlr4.Runtime.RuleContext parent = context?.Parent;

            if (parent == null)
                return -1;

            for (int i = 0; i < parent.ChildCount; i++) {
                if (parent.GetChild(i) == context) {
                    return i;
                }
            }

            return -1;
        }
    }
}