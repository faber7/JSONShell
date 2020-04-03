using Generated;
using Serilog;

namespace Skell.Interpreter
{
    class SkellVisitor : SkellBaseVisitor<Skell.Data.SkellData>
    {
        Skell.Data.Boolean defaultReturnValue = new Skell.Data.Boolean(true);
        private static ILogger logger;

        public SkellVisitor()
        {
            logger = Log.ForContext<SkellVisitor>();
        }

        /// <remarks>
        /// program : statement+ ;
        /// </remarks>
        override public Skell.Data.SkellData VisitProgram(SkellParser.ProgramContext context)
        {
            Skell.Data.SkellData lastResult = defaultReturnValue;
            foreach (var statement in context.statement()) {
                logger.Verbose("Visiting:\n" + statement.ToStringTree());
                lastResult = Visit(statement);
                if (lastResult is Skell.Data.Array) {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType().ToString()} and length {((Skell.Data.Array)lastResult).Length}");
                } else {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType().ToString()}");
                }
                System.Console.WriteLine(lastResult);
            }
            return lastResult;
        }

        /// <remarks>
        /// statement : expression ;
        /// </remarks>
        override public Skell.Data.SkellData VisitStatement(SkellParser.StatementContext context)
        {
            return Visit(context.expression());
        }

        /// <remarks>
        /// expression : eqExpr ;
        /// </remarks>
        override public Skell.Data.SkellData VisitExpression(SkellParser.ExpressionContext context)
        {
            return Visit(context.eqExpr());
        }

        /// <remarks>
        /// eqExpr : relExpr ((OP_NE | OP_EQ) relExpr)? ;
        /// </remarks>
        override public Skell.Data.SkellData VisitEqExpr(SkellParser.EqExprContext context)
        {
            Skell.Data.SkellData result = VisitRelExpr(context.relExpr(0));

            if (context.relExpr(1) != null) {
                Skell.Data.SkellData next = VisitRelExpr(context.relExpr(1));
                var op = (Antlr4.Runtime.IToken) Utility.GetLeftSibling(context.relExpr(1)).Payload;
                if (op.Type == SkellLexer.OP_NE) {
                    return new Skell.Data.Boolean(!(result.Equals(next)));
                } else {
                    return new Skell.Data.Boolean(result.Equals(next));
                }
            }
            return result;
        }

        /// <remarks>
        /// relExpr : addExpr ((OP_GT | OP_GE | OP_LT | OP_LE) addExpr)? ;
        /// </remarks>
        override public Skell.Data.SkellData VisitRelExpr(SkellParser.RelExprContext context)
        {
            Skell.Data.SkellData result = VisitAddExpr(context.addExpr(0));

            if (context.addExpr(1) != null) {
                Skell.Data.SkellData next = VisitAddExpr(context.addExpr(1));
                var op = (Antlr4.Runtime.IToken) Utility.GetLeftSibling(context.addExpr(1)).Payload;
                if (op.Type == SkellLexer.OP_GT) {
                    return (Skell.Data.Number) result > (Skell.Data.Number) next;
                } else if (op.Type == SkellLexer.OP_GE) {
                    return (Skell.Data.Number) result >= (Skell.Data.Number) next;
                } else if (op.Type == SkellLexer.OP_LT) {
                    return (Skell.Data.Number) result < (Skell.Data.Number) next;
                } else {
                    return (Skell.Data.Number) result <= (Skell.Data.Number) next;
                }
            }
            return result;
        }

        /// <remarks>
        /// addExpr : mulExpr ((OP_SUB | OP_ADD) mulExpr)* ;
        /// </remarks>
        override public Skell.Data.SkellData VisitAddExpr(SkellParser.AddExprContext context)
        {
            int i = 0;
            Skell.Data.SkellData result = VisitMulExpr(context.mulExpr(i));

            i++;
            while (context.mulExpr(i) != null) {
                if (!(result is Skell.Data.Number)) {
                    throw new System.NotImplementedException();
                }
                Skell.Data.SkellData next = VisitMulExpr(context.mulExpr(i));
                if (!(next is Skell.Data.Number)) {
                    throw new System.NotImplementedException();
                }
                var op = (Antlr4.Runtime.IToken) Utility.GetLeftSibling(context.mulExpr(i)).Payload;
                if (op.Type == SkellLexer.OP_SUB) {
                    result = (Skell.Data.Number) result - (Skell.Data.Number) next;
                } else {
                    result = (Skell.Data.Number) result + (Skell.Data.Number) next;
                }
                i++;
            }
            return result;
        }

        /// <remarks>
        /// mulExpr : unary ((OP_DIV | OP_MUL) unary)* ;
        /// </remarks>
        override public Skell.Data.SkellData VisitMulExpr(SkellParser.MulExprContext context)
        {
            int i = 0;
            Skell.Data.SkellData result = VisitUnary(context.unary(i));

            i++;
            while (context.unary(i) != null) {
                if (!(result is Skell.Data.Number)) {
                    throw new System.NotImplementedException();
                }
                Skell.Data.SkellData next = VisitUnary(context.unary(i));
                if (!(next is Skell.Data.Number)) {
                    throw new System.NotImplementedException();
                }
                var op = (Antlr4.Runtime.IToken) Utility.GetLeftSibling(context.unary(i)).Payload;                
                if (op.Type == SkellLexer.OP_DIV) {
                    result = (Skell.Data.Number) result / (Skell.Data.Number) next;
                } else {
                    result = (Skell.Data.Number) result * (Skell.Data.Number) next;
                }
                i++;
            }
            return result;
        }

        /// <remarks>
        /// unary : (OP_NOT | OP_SUB) unary
        ///       | primary
        ///       ;
        /// </remarks>
        override public Skell.Data.SkellData VisitUnary(SkellParser.UnaryContext context)
        {
            if (context.unary() != null) {
                var op = (Antlr4.Runtime.IToken) Utility.GetLeftSibling(context.unary()).Payload;
                if (op.Type == SkellLexer.OP_NOT) {
                    Skell.Data.Boolean boolUnary = new Skell.Data.Boolean(VisitUnary(context.unary()).ToString());
                    return !boolUnary;
                }
                Skell.Data.Number numUnary = new Skell.Data.Number(VisitUnary(context.unary()).ToString());
                return -numUnary;
            }
            return VisitPrimary(context.primary());
        }

        /// <remarks>
        /// primary : term
        ///         | LPAREN expression RPAREN
        ///         ;
        /// </remarks>
        override public Skell.Data.SkellData VisitPrimary(SkellParser.PrimaryContext context)
        {
            if (context.term() != null) {
                return VisitTerm(context.term());
            }
            return VisitExpression(context.expression());
        }

        /// <remarks>
        /// term : value | IDENTIFIER ;
        /// </remarks>
        override public Skell.Data.SkellData VisitTerm(SkellParser.TermContext context)
        {
            if (context.IDENTIFIER() != null) {
                // handle identifier
                throw new System.NotImplementedException();
            }
            return VisitValue(context.value());
        }
        
        /// <remarks>
        /// value : object | array | STRING | NUMBER | bool ;
        /// </remarks>
        override public Skell.Data.SkellData VisitValue(SkellParser.ValueContext context)
        {
            if (context.@object() != null) {
                return VisitObject(context.@object());
            }
            if (context.array() != null) {
                return VisitArray(context.array());
            }
            if (context.STRING() != null) {
                string contents = context.STRING().GetText();
                contents = contents.Remove(0,1);
                contents = contents.Remove(contents.Length - 1,1);
                return new Skell.Data.String(contents);
            }
            if (context.NUMBER() != null) {
                return new Skell.Data.Number(context.NUMBER().GetText());
            }
            return new Skell.Data.Boolean(context.@bool().GetText());
        }

        /// <remarks>
        /// array : LSQR value (SYM_COMMA value)* RSQR
        ///       | LSQR RSQR
        ///       ;
        /// </remarks>
        override public Skell.Data.SkellData VisitArray(SkellParser.ArrayContext context)
        {
            SkellParser.ValueContext[] values = context.value();
            Skell.Data.SkellData[] contents = new Skell.Data.SkellData[values.Length];
            for (int i = 0; i < values.Length; i++) {
                contents[i] = VisitValue(values[i]);
            }
            return new Skell.Data.Array(contents);
        }

        /// <remarks>
        /// object : LCURL pair (SYM_COMMA pair)* RCURL
        ///        | LCURL RCURL
        ///        ;
        /// </remarks>
        override public Skell.Data.SkellData VisitObject(SkellParser.ObjectContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    static class Utility
    {
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