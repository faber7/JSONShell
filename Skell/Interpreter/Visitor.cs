using Skell.Generated;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Antlr4.Runtime;
using System;

namespace Skell.Interpreter
{
    internal class Visitor : SkellBaseVisitor<Skell.Types.ISkellType>
    {
        private readonly Skell.Types.Null defaultReturnValue = new Skell.Types.Null();
        private static ILogger logger;
        private ICharStream tokenSource;

        private State state;

        public Visitor()
        {
            logger = Log.ForContext<Visitor>();
            state = new State();
        }

        public void SetSource(ICharStream source)
        {
            tokenSource = source;
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
        /// statement : EOL | programExec EOL | declaration EOL | expression EOL | control ;
        /// </summary>
        override public Skell.Types.ISkellType VisitStatement(SkellParser.StatementContext context)
        {
            if (context.expression() != null) {
                return VisitExpression(context.expression());
            } else if (context.control() != null) {
                return VisitControl(context.control());
            } else if (context.declaration() != null) {
                return VisitDeclaration(context.declaration());
            } else if (context.programExec() != null) {
                return VisitProgramExec(context.programExec());
            } else {
                return defaultReturnValue;
            }
        }

        /// <summary>
        /// programExec : SYM_DOLLAR ~EOL* ;
        /// </summary>
        override public Skell.Types.ISkellType VisitProgramExec(SkellParser.ProgramExecContext context)
        {
            string execString = tokenSource.GetText(
                    new Antlr4.Runtime.Misc.Interval(
                        context.Start.StartIndex, context.Stop.StopIndex
                    )
                )
                .Substring(1);
            List<string> argsList = execString.Split(' ').ToList();
            if (argsList.First().Length == 0) {
                argsList.RemoveAt(0);
            }

            Process process = new Process();

            process.StartInfo.FileName = argsList.First();
            argsList = argsList.Skip(1).Select(
                str => {
                    if (str.StartsWith('$')) {
                        string name = str.Substring(1);
                        return state.context.Get(name).ToString();
                    }
                    return str;
                }
            ).ToList();

            logger.Information($"Running command: {execString}");
            process.StartInfo.Arguments = string.Join(" ", argsList);
            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;

            return new Skell.Types.Number(exitCode);
        }

        /// <summary>
        /// statementBlock : LCURL statement* RCURL ;
        /// </summary>
        override public Skell.Types.ISkellType VisitStatementBlock(SkellParser.StatementBlockContext context)
        {
            for (int i = 0; i < context.statement().Length; i++) {
                var result = VisitStatement(context.statement(i));
                if (state.has_returned()) {
                    // do not unset state.has_returned() here as it is the caller's job
                    return result;
                }
            }
            return defaultReturnValue;
        }

        /// <summary>
        /// declaration : KW_LET IDENTIFIER OP_ASSGN (expression | function);
        /// function : LPAREN RPAREN statementBlock
        ///          | LPAREN functionArg (SYM_COMMA functionArg)* statementBlock
        ///          ;
        /// functionArg : typeName IDENTIFIER ;
        /// </summary>
        override public Skell.Types.ISkellType VisitDeclaration(SkellParser.DeclarationContext context)
        {
            string name = Utility.GetIdentifierName(context.IDENTIFIER());

            if (context.expression() != null) {
                if (!state.functions.Exists(name)) {
                    var exp = VisitExpression(context.expression());
                    state.context.Set(name, exp);
                } else {
                    throw new Skell.Problems.InvalidDefinition(name, state.functions.Get(name));
                }
            } else {
                if (!state.context.Exists(name)) {
                    var fn = Utility.Function.Get(state, name);
                    fn.AddUserDefinedLambda(context.function());
                } else {
                    throw new Skell.Problems.InvalidDefinition(name, state.context.Get(name));
                }
            }

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
                    throw new Skell.Problems.UnexpectedType(result, typeof(Skell.Types.Number));
                }
                Skell.Types.ISkellType next = VisitMulExpr(context.mulExpr(i));
                if (!(next is Skell.Types.Number)) {
                    throw new Skell.Problems.UnexpectedType(next, typeof(Skell.Types.Number));
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
                    throw new Skell.Problems.UnexpectedType(result, typeof(Skell.Types.Number));
                }
                Skell.Types.ISkellType next = VisitUnary(context.unary(i));
                if (!(next is Skell.Types.Number)) {
                    throw new Skell.Problems.UnexpectedType(result, typeof(Skell.Types.Number));
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
            if (cont.value) {
                return VisitStatementBlock(context.statementBlock());
            }
            return defaultReturnValue;
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
                Skell.Types.ISkellType result = defaultReturnValue;

                var last = state.ENTER_RETURNABLE_CONTEXT($"for {varName} in {arr}");

                foreach (Skell.Types.ISkellType data in arr) {
                    state.context.Set(varName, data);
                    result = VisitStatementBlock(context.statementBlock());
                    if (state.has_returned())
                    {
                        logger.Debug("For loop returned with " + result);
                        state.end_return();
                        return result;
                    }
                }

                state.EXIT_RETURNABLE_CONTEXT(last);
                return defaultReturnValue;
            }
            throw new Skell.Problems.UnexpectedType(primary, typeof(Skell.Types.Array));
        }

        /// <summary>
        /// returnControl : KW_RETURN expression? ;
        /// </summary>
        /// <remark>
        /// Sets FLAG_RETURN before returning
        /// </remark>
        override public Skell.Types.ISkellType VisitReturnControl(SkellParser.ReturnControlContext context)
        {
            if (!state.can_return()) {
                logger.Warning("Tried to return from non-returnable area");
                throw new System.NotImplementedException();
            }
            Skell.Types.ISkellType retval = defaultReturnValue;
            if (context.expression() != null) {
                retval = VisitExpression(context.expression());
            }
            state.start_return();
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
        /// <remark>
        /// term is executed if it refers to a function
        /// </remark>
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
                    index = state.context.Get(Utility.GetIdentifierName(context.IDENTIFIER()));
                }
                if (term is Skell.Types.Object obj) {
                    return obj.GetMember(index);
                } else if (term is Skell.Types.Array arr) {
                    return arr.GetMember(index);
                }
                throw new Skell.Problems.UnexpectedType(term, typeof(Skell.Types.ISkellIndexableType));
            } else if (context.term() != null) {
                Skell.Types.ISkellType term = VisitTerm(context.term());
                if (term is Skell.Types.Function fn) {
                    var args = new List<Tuple<int, Skell.Types.ISkellType>>();
                    return Utility.Function.ExecuteFunction(
                        this,
                        state,
                        fn,
                        args
                    );             
                }
                return term;
            } else if (context.expression() != null) {
                return VisitExpression(context.expression());
            } else {
                return VisitFnCall(context.fnCall());
            }
        }

        /// <summary>
        /// fnCall : IDENTIFIER fnArg+ ;
        /// fnArg : IDENTIFIER SYM_COLON expression ;
        /// </summary>
        /// <remark>
        /// If there is no argument, the call is seen as a term instead of a fnCall
        /// </remark>
        override public Skell.Types.ISkellType VisitFnCall(SkellParser.FnCallContext context)
        {
            string name = Utility.GetIdentifierName(context.IDENTIFIER());
            Skell.Types.Function fn = Utility.Function.Get(state, name);
            var args = new List<Tuple<int, Skell.Types.ISkellType>>();
            int i = 0;
            foreach (var arg in context.expression()) {
                args.Add(new Tuple<int, Types.ISkellType>(i, VisitExpression(arg)));
                i++;
            }
            return Utility.Function.ExecuteFunction(
                this,
                state,
                fn,
                args
            );
        }

        /// <summary>
        /// term : value
        ///      | IDENTIFIER
        ///      ;
        /// </summary>
        override public Skell.Types.ISkellType VisitTerm(SkellParser.TermContext context)
        {
            if (context.IDENTIFIER() != null) {
                string name = Utility.GetIdentifierName(context.IDENTIFIER());
                if (state.functions.Exists(name)) {
                    return Utility.Function.Get(state, name);
                }
                return state.context.Get(name);
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
        /// array : LSQR EOL? (term (SYM_COMMA EOL? term)*)? RSQR ;
        /// </summary>
        override public Skell.Types.ISkellType VisitArray(SkellParser.ArrayContext context)
        {
            SkellParser.TermContext[] values = context.term();
            Skell.Types.ISkellType[] contents = new Skell.Types.ISkellType[values.Length];
            for (int i = 0; i < values.Length; i++) {
                contents[i] = VisitTerm(values[i]);
            }
            return new Skell.Types.Array(contents);
        }

        /// <summary>
        /// object : LCURL EOL? (pair (SYM_COMMA EOL? pair)*)? RCURL ;
        /// pair : STRING SYM_COLON term ;
        /// </summary>
        override public Skell.Types.ISkellType VisitObject(SkellParser.ObjectContext context)
        {
            Skell.Types.String[] keys = new Skell.Types.String[context.pair().Length];
            Skell.Types.ISkellType[] values = new Skell.Types.ISkellType[context.pair().Length];

            for (int i = 0; i < context.pair().Length; i++) {
                var pair = context.pair(i);
                keys[i] = Utility.GetString(pair.STRING());
                values[i] = VisitTerm(pair.term());
            }

            return new Skell.Types.Object(keys, values);
        }
    }
}