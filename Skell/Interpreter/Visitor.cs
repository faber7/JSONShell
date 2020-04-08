using Skell.Generated;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Skell.Interpreter
{
    internal class Visitor : SkellBaseVisitor<Skell.Types.ISkellType>
    {
        private readonly Skell.Types.Null defaultReturnValue = new Skell.Types.Null();
        private static ILogger logger;

        private readonly List<Context> contexts;
        private Context current_context;
        private bool flag_return;
        private bool flag_returned;

        public Visitor()
        {
            logger = Log.ForContext<Visitor>();
            Context global = new Context("GLOBAL");
            contexts = new List<Context>();
            contexts.Append(global);
            current_context = global;
        }

        /// <summary>
        /// Safely set flag_return
        /// The returned value must be passed to EXIT_RETURNABLE_STATE() even if nothing was returned
        /// </summary>
        private bool ENTER_RETURNABLE_STATE() {
            logger.Debug("Entering returnable state from current: " + flag_return);
            var ret = flag_return;
            flag_return = true;
            return ret;
        }

        /// <summary>
        /// Unsets flag_return
        /// Pass the returned value of ENTER_RETURNABLE_STATE(), even if nothing was returned
        /// </summary>
        private void EXIT_RETURNABLE_STATE(bool last) {
            logger.Debug("Exiting from current returnable state to: " + last);
            flag_return = last;
        }

        /// <summary>
        /// Safely switch to a new context
        /// The returned context is the current context
        /// Pass it to EXIT_CONTEXT() to switch back
        /// </summary>
        private Context ENTER_CONTEXT(string name) {
            logger.Debug($"Entering a new context from {current_context.contextName}");
            Context context = new Context("{" + name + "}");
            contexts.Append(context);
            var last_context = current_context;
            current_context = context;
            return last_context;
        }

        /// <summary>
        /// Exit from the current context to the given context
        /// </summary>
        private void EXIT_CONTEXT(Context prevContext) {
            logger.Debug($"Exiting context {current_context.contextName} to {prevContext.contextName}");
            contexts.Remove(current_context);
            current_context = prevContext;
        }

        /// <summary>
        /// program : statement+ ;
        /// </summary>
        override public Skell.Types.ISkellType VisitProgram(SkellParser.ProgramContext context)
        {
            Skell.Types.ISkellType lastResult = defaultReturnValue;
            foreach (var statement in context.statement()) {
                lastResult = Visit(statement);
                if (lastResult is Skell.Types.Array arr) {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()} and length {arr.Count()}");
                } else if (lastResult is Skell.Types.Object obj) {
                    logger.Debug($"Result is an object:\n {obj}");
                } else if (!(lastResult is Skell.Types.Null)) {
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()}");
                }
            }
            return lastResult;
        }

        /// <summary>
        /// statement : EOL | declaration EOL | expression EOL | control ;
        /// </summary>
        override public Skell.Types.ISkellType VisitStatement(SkellParser.StatementContext context)
        {
            if (context.expression() != null) {
                return VisitExpression(context.expression());
            } else if (context.control() != null) {
                return VisitControl(context.control());
            } else if (context.declaration() != null) {
                return VisitDeclaration(context.declaration());
            } else {
                return defaultReturnValue;
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
                if (flag_returned) {
                    // do not unset flag_returned here as it is the caller's job
                    return lastResult;
                }
            }
            return lastResult;
        }

        /// <summary>
        /// declaration : KW_LET IDENTIFIER
        ///             | KW_LET IDENTIFIER OP_ASSGN (expression | lambda);
        /// </summary>
        override public Skell.Types.ISkellType VisitDeclaration(SkellParser.DeclarationContext context)
        {
            string name = Utility.GetIdentifierName(context.IDENTIFIER());

            if (context.OP_ASSGN() == null) {
                current_context.Set(name, new Skell.Types.Object());
            } else if (context.expression() != null) {
                var exp = VisitExpression(context.expression());
                current_context.Set(name, exp);
            } else {
                var lambda = VisitLambda(context.lambda());
                current_context.Set(name, lambda);
            }

            return defaultReturnValue;
        }

        /// <summary>
        /// lambda : LPAREN RPAREN statementBlock
        ///        | LPAREN lambdaArg (SYM_COMMA lambdaArg)* statementBlock
        ///        ;
        /// lambdaArg : typeName IDENTIFIER ;
        /// </summary>
        override public Skell.Types.ISkellType VisitLambda(SkellParser.LambdaContext context)
        {
            return new Skell.Types.Lambda(context);
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
                    switch (op.Type) {
                        case SkellLexer.OP_GE:
                            return r >= n;
                        case SkellLexer.OP_GT:
                            return r > n;
                        case SkellLexer.OP_LE:
                            return r < n;
                        case SkellLexer.OP_LT:
                            return r <= n;
                    }
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
                    throw new Skell.Error.UnexpectedType(result, typeof(Skell.Types.Number));
                }
                Skell.Types.ISkellType next = VisitMulExpr(context.mulExpr(i));
                if (!(next is Skell.Types.Number)) {
                    throw new Skell.Error.UnexpectedType(next, typeof(Skell.Types.Number));
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
                    throw new Skell.Error.UnexpectedType(result, typeof(Skell.Types.Number));
                }
                Skell.Types.ISkellType next = VisitUnary(context.unary(i));
                if (!(next is Skell.Types.Number)) {
                    throw new Skell.Error.UnexpectedType(result, typeof(Skell.Types.Number));
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
        /// control : ifControl | forControl | returnControl ;
        /// </summary>
        override public Skell.Types.ISkellType VisitControl(SkellParser.ControlContext context)
        {
            if (context.ifControl() != null) {
                return VisitIfControl(context.ifControl());
            } else if (context.forControl() != null) {
                return VisitForControl(context.forControl());
            }
            return VisitReturnControl(context.returnControl());
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
        /// forControl : KW_FOR IDENTIFIER KW_IN expression statementBlock ;
        /// </summary>
        override public Skell.Types.ISkellType VisitForControl(SkellParser.ForControlContext context)
        {
            var primary = VisitExpression(context.expression());
            if (primary is Skell.Types.Array arr) {
                string varName = Utility.GetIdentifierName(context.IDENTIFIER());
                Skell.Types.ISkellType lastResult = defaultReturnValue;

                var last_context = ENTER_CONTEXT($"for {arr}");
                var last_return = ENTER_RETURNABLE_STATE();

                foreach (Skell.Types.ISkellType data in arr) {
                    current_context.Set(varName, data);
                    lastResult = VisitStatementBlock(context.statementBlock());
                    if (flag_returned)
                    {
                        logger.Debug("For loop returned with " + lastResult);
                        flag_returned = false;
                        break;
                    }
                }

                EXIT_RETURNABLE_STATE(last_return);
                EXIT_CONTEXT(last_context);
                return lastResult;
            }
            throw new Skell.Error.UnexpectedType(primary, typeof(Skell.Types.Array));
        }

        /// <summary>
        /// returnControl : KW_RETURN expression? ;
        /// </summary>
        /// <remark>
        /// Sets FLAG_RETURN before returning
        /// </remark>
        override public Skell.Types.ISkellType VisitReturnControl(SkellParser.ReturnControlContext context)
        {
            if (!flag_return) {
                logger.Warning("Tried to return from non-returnable area");
                throw new System.NotImplementedException();
            }
            Skell.Types.ISkellType retval = defaultReturnValue;
            if (context.expression() != null) {
                retval = VisitExpression(context.expression());
            }
            logger.Debug($"Returning with value {retval}");
            flag_returned = true;
            return retval;
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
        ///         | fnCall
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
                    index = current_context.Get(Utility.GetIdentifierName(context.IDENTIFIER()));
                }
                if (term is Skell.Types.Object obj) {
                    return obj.GetMember(index);
                } else if (term is Skell.Types.Array arr) {
                    return arr.GetMember(index);
                }
                throw new Skell.Error.UnexpectedType(term, typeof(Skell.Types.ISkellIndexableType));
            } else if (context.term() != null) {
                return VisitTerm(context.term());
            } else if (context.expression() != null) {
                return VisitExpression(context.expression());
            } else {
                return VisitFnCall(context.fnCall());
            }
        }

        /// <summary>
        /// fnCall : IDENTIFIER LPAREN RPAREN
        ///        | IDENTIFIER LPAREN fnArg (SYM_COMMA fnArg)* RPAREN
        ///        ;
        /// fnArg : IDENTIFIER SYM_COLON expression ;
        /// </summary>
        override public Skell.Types.ISkellType VisitFnCall(SkellParser.FnCallContext context)
        {
            string name = Utility.GetIdentifierName(context.IDENTIFIER());
            Skell.Types.Lambda lambda = Utility.GetLambda(name, current_context);
            Dictionary<string, Skell.Types.ISkellType> args = new Dictionary<string, Types.ISkellType>();
            foreach (var arg in context.fnArg()) {
                string argName = Utility.GetIdentifierName(arg.IDENTIFIER());
                Skell.Types.ISkellType argValue = VisitExpression(arg.expression());

                args.Add(argName, argValue);
            }

            var extra = Utility.GetExtraArgs(lambda.argsList, args);
            var invalid = Utility.GetInvalidArgs(lambda.argsList, args);

            if (extra.Count > 0 || invalid.Count > 0) {
                throw new System.NotImplementedException();
            }

            var last_context = ENTER_CONTEXT($"{name}");
            var last_return = ENTER_RETURNABLE_STATE();

            foreach (var arg in args) {
                current_context.Set(arg.Key, arg.Value);
            }
            var result = VisitStatementBlock(lambda.statementBlock);
            if (flag_returned) {
                logger.Debug($"{name}() returned {result}");
                flag_returned = false;
            }

            EXIT_RETURNABLE_STATE(last_return);
            EXIT_CONTEXT(last_context);

            return result;
        }

        /// <summary>
        /// term : value
        ///      | IDENTIFIER
        ///      ;
        /// </summary>
        override public Skell.Types.ISkellType VisitTerm(SkellParser.TermContext context)
        {
            if (context.IDENTIFIER() != null) {
                return current_context.Get(Utility.GetIdentifierName(context.IDENTIFIER()));
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
        /// Returns the token for the given typeName context
        /// </summary>
        /// <remark>
        /// typeName : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTokenOfTypeName(SkellParser.TypeNameContext context)
        {
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }

        /// <summary>
        /// Returns the lambda if it exists
        /// </summary>
        public static Skell.Types.Lambda GetLambda(string name, Context context)
        {
            Skell.Types.ISkellType data = context.Get(name);
            if (data is Skell.Types.Lambda lambda) {
                return lambda;
            }
            throw new Skell.Error.UnexpectedType(data, typeof(Skell.Types.Lambda));
        }

        /// <summary>
        /// Returns true if the data matches the given token type
        /// </summary>
        private static bool MatchType(Skell.Types.ISkellType data, Antlr4.Runtime.IToken token)
        {
            if (data is Skell.Types.Array && token.Type == SkellLexer.TYPE_ARRAY) {
                return true;
            } else if (data is Skell.Types.Boolean && token.Type == SkellLexer.TYPE_BOOL) {
                return true;
            } else if (data is Skell.Types.Number && token.Type == SkellLexer.TYPE_NUMBER) {
                return true;
            } else if (data is Skell.Types.Object && token.Type == SkellLexer.TYPE_OBJECT) {
                return true;
            } else if (data is Skell.Types.String && token.Type == SkellLexer.TYPE_STRING) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the extra arguments that are to be passed to the function.
        /// Use with ValidateArgs().
        /// </summary>
        public static Dictionary<string,Skell.Types.ISkellType> GetExtraArgs(
            Dictionary<string, Antlr4.Runtime.IToken> lambdaArgs,
            Dictionary<string, Skell.Types.ISkellType> args
        )
        {
            Dictionary<string, Skell.Types.ISkellType> extra = new Dictionary<string, Skell.Types.ISkellType>();
            if (args.Count > lambdaArgs.Count) {
                foreach (var extraArg in args.Where(
                    pair => !lambdaArgs.Keys.Contains(pair.Key)
                ).ToArray()) {
                    extra.Add(extraArg.Key, extraArg.Value);
                }
            }
            return extra;
        }

        /// <summary>
        /// Retrieves the arguments that are either not passed to the function
        /// or are passed but do not have the same type.
        /// Use with GetExtraArgs().
        /// </summary>
        public static Dictionary<string, Antlr4.Runtime.IToken> GetInvalidArgs(
            Dictionary<string, Antlr4.Runtime.IToken> lambdaArgs,
            Dictionary<string, Skell.Types.ISkellType> args
        )
        {
            Dictionary<string, Antlr4.Runtime.IToken> ret = new Dictionary<string, Antlr4.Runtime.IToken>();
            var arguments = lambdaArgs.Keys.ToArray();
            for (int i = 0; i < arguments.Length; i++) {
                var name = arguments[i];
                var type = lambdaArgs[name];
                if (!(args.Keys.Contains(name) && MatchType(args[name], type))) {
                    ret.Add(name, type);
                }
            }
            return ret;
        }

        /// <summary>
        /// Evaluates an expression and returns a Skell.Types.Boolean
        /// </summary>
        public static Skell.Types.Boolean EvaluateExpr(Visitor parser, SkellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            if (!(expressionResult is Skell.Types.Boolean)) {
                throw new Skell.Error.UnexpectedType(expressionResult, typeof(Skell.Types.Boolean));
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