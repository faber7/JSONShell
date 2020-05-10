using Skell.Generated;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Antlr4.Runtime;
using System;

namespace Skell.Interpreter
{
    public class Visitor : SkellBaseVisitor<Skell.Types.ISkellReturnable>
    {
        private readonly Skell.Types.None defaultReturnValue = new Skell.Types.None();
        private static ILogger logger;
        public ICharStream tokenSource;

        private State state;

        public Visitor(State st)
        {
            logger = Log.ForContext<Visitor>();
            state = st;
        }

        /// <summary>
        /// program : programStatement+ ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitProgram(SkellParser.ProgramContext context)
        {
            var statements = context.programStatement();

            Skell.Types.ISkellReturnable lastResult = defaultReturnValue;
            foreach (var statement in statements) {
                lastResult = VisitProgramStatement(statement);
                if (lastResult is Skell.Types.String str)
                    logger.Debug($"Result: \"{lastResult}\" of type {lastResult.GetType()}");
                else if (lastResult is Skell.Types.Array arr)
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()} and length {arr.Count()}");
                else if (lastResult is Skell.Types.Object obj)
                    logger.Debug($"Result is an object:\n {obj}");
                else if (!(lastResult is Skell.Types.None))
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()}");
            }
            return lastResult;
        }

        /// <summary>
        /// programStatement : namespace EOL
        ///                  | statement
        ///                  ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitProgramStatement(SkellParser.ProgramStatementContext context)
        {
            var ctx_ns = context.@namespace();
            var ctx_stmt = context.statement();

            if (ctx_ns != null) {
                var ns = new Skell.Types.Namespace(".", ctx_ns, this);
                state.Namespaces.Register(ns);
                return defaultReturnValue;
            } else
                return VisitStatement(ctx_stmt);
        }

        /// <summary>
        /// statement : EOL
        ///           | namespaceLoad EOL
        ///           | programExec EOL
        ///           | declaration EOL
        ///           | expression EOL
        ///           | control
        ///           ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitStatement(SkellParser.StatementContext context)
        {
            var ctx_nsLoad = context.namespaceLoad();
            var ctx_progExec = context.programExec();
            var ctx_decl = context.declaration();
            var ctx_expr = context.expression();
            var ctx_ctrl = context.control();

            if (ctx_nsLoad != null)
                return VisitNamespaceLoad(ctx_nsLoad);
            else if (ctx_progExec != null)
                return VisitProgramExec(ctx_progExec);
            else if (ctx_decl != null)
                return VisitDeclaration(ctx_decl);
            else if (ctx_expr != null) {
                var ret = VisitExpression(ctx_expr);
                if (ret is Skell.Types.Property)
                    return ((Skell.Types.Property) ret).value;
                return ret;
            } else if (ctx_ctrl != null)
                return VisitControl(ctx_ctrl);

            return defaultReturnValue;
        }

        /// <summary>
        /// namespaceLoad : KW_USING STRING (KW_AS IDENTIFIER)? ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitNamespaceLoad(SkellParser.NamespaceLoadContext context)
        {
            var ctx_str = context.STRING();
            var ctx_id = context.IDENTIFIER();

            var name = Utility.GetString(ctx_str.GetText(), this);
            var ns = Utility.LoadNamespace(
                    name.contents,
                    ctx_id != null ? ctx_id.GetText() : "",
                    this
                );
            if (state.Names.DefinedAs(ns.name, typeof(Skell.Types.Namespace)))
                logger.Debug($"Overwriting namespace {ns.name}");

            state.Namespaces.Register(ns);
            return defaultReturnValue;
        }

        /// <summary>
        /// programExec : SYM_DOLLAR ~EOL* ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitProgramExec(SkellParser.ProgramExecContext context)
        {
            string execString = tokenSource.GetText(
                    new Antlr4.Runtime.Misc.Interval(
                        context.Start.StartIndex, context.Stop.StopIndex
                    )
                )
                .Substring(1);
            List<string> argsList = execString.Split(' ').ToList();
            if (argsList.First().Length == 0)
                argsList.RemoveAt(0);

            List<string> nargs = new List<string>();

            // Join any arguments if they are valid substitutions
            for (int i = 0; i < argsList.Count; i++) {
                bool added = false;
                var arg = argsList[i];
                for (int j = i + 1; j < argsList.Count; j++) {
                    if (arg.StartsWith("\"") && argsList[j].EndsWith("\"")) {
                        nargs.Add(String.Join(" ", argsList.GetRange(i, j)));
                        i = j + 1;
                        added = true;
                        break;
                    } else if (arg.StartsWith('`') && argsList[j].EndsWith('`')) {
                        nargs.Add(String.Join(" ", argsList.GetRange(i, j)));
                        i = j + 1;
                        added = true;
                        break;
                    }
                }
                if (!added)
                    nargs.Add(arg);
            }

            argsList = nargs;

            Process process = new Process();

            process.StartInfo.FileName = argsList.First();
            argsList = argsList.Skip(1).Select(
                str => {
                    // perform any substitutions
                    if (str.StartsWith("\"") && str.EndsWith("\""))
                        return Utility.GetString(str, this).ToString();
                    else if (str.StartsWith('`') && str.EndsWith('`'))
                        return str.Substring(1, str.Length - 2);

                    return str;
                }
            ).ToList();

            logger.Information($"Running command: {execString}");
            process.StartInfo.Arguments = string.Join(" ", argsList);
            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            logger.Information($"Process {process.StartInfo.FileName} exited with return code {exitCode}");

            return new Skell.Types.Number(exitCode);
        }

        /// <summary>
        /// statementBlock : LCURL statement* RCURL ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitStatementBlock(SkellParser.StatementBlockContext context)
        {
            var statements = context.statement();
            foreach (var statement in statements) {
                var result = VisitStatement(statement);
                if (state.has_returned())
                    // do not unset state.has_returned() here as it is the caller's job
                    return result;
            }
            return defaultReturnValue;
        }

        /// <summary>
        /// declaration : KW_LET primary OP_ASSGN (expression | function);
        /// function : LPAREN RPAREN statementBlock
        ///          | LPAREN functionArg (SYM_COMMA functionArg)* statementBlock
        ///          ;
        /// functionArg : TypeSpecifier IDENTIFIER ;
        ///
        /// primary : (term | fnCall)
        ///         | LPAREN expression RPAREN
        ///         ;
        /// term : value
        ///      | IDENTIFIER
        ///      | namespacedIdentifier
        ///      | term LSQR (STRING | NUMBER | IDENTIFIER) RSQR
        ///      ;
        /// </summary>
        /// <remark>
        /// Declarations using identifiers and indexed objects are handled here
        /// </remark>
        override public Skell.Types.ISkellReturnable VisitDeclaration(SkellParser.DeclarationContext context)
        {
            var ctx_expression = context.expression();
            var ctx_function = context.function();

            var ctx_primary = context.primary();
            var ctx_LHS = ctx_primary.term();

            // if primary is a term, handle it here as there is no way to get the reference to an object
            if (ctx_LHS != null) {
                var ctx_id = ctx_LHS.IDENTIFIER();
                var ctx_indexable = ctx_LHS.term();

                if (ctx_id != null) {
                    string name = ctx_id.GetText();
                    if (ctx_expression != null) {
                        Skell.Types.ISkellReturnable expression = VisitExpression(ctx_expression);
                        if (expression is Skell.Types.Property prop)
                            expression = prop.value;
                        Source src_expression = new Source(ctx_expression.Start, ctx_expression.Stop);

                        if (state.Names.Available(name) || state.Names.DefinedAs(name, typeof(Skell.Types.ISkellType)))
                            state.Variables.Set(name, Utility.GetSkellType(src_expression, expression));
                        else
                            throw new Skell.Problems.InvalidDefinition(src_expression, name, state);
                    } else {
                        Skell.Types.UserDefinedLambda lambda = new Skell.Types.UserDefinedLambda(ctx_function);
                        Source src_function = new Source(ctx_function.Start, ctx_function.Stop);

                        if (state.Names.Available(name) || state.Names.DefinedAs(name, typeof(Skell.Types.Function))) {
                            var fn = Utility.Function.Get(state, name);
                            fn.AddUserDefinedLambda(ctx_function);
                        } else
                            throw new Skell.Problems.InvalidDefinition(src_function, name, state);
                    }
                } else if (ctx_indexable != null) {
                    // This block is almost similar to VisitTerm()'s ctx_term != null block
                    // id is the child of ctx_term here instead of context
                    // instead of returning the value we replace the value at the index instead
                    // if the indexable is a function it is not executed
                    // if the RHS is a function an error is thrown

                    var id = (Antlr4.Runtime.Tree.ITerminalNode) ctx_LHS.GetChild(2);
                    var text = id.Symbol.Text;
                    var src = new Source(id.Symbol);

                    Skell.Types.ISkellType index;

                    if (id.Symbol.Type == SkellLexer.STRING)
                        index = Utility.GetString(text, this);
                    else if (id.Symbol.Type == SkellLexer.NUMBER)
                        index = new Skell.Types.Number(text);
                    else 
                        index = Utility.GetIdentifer(src, text, this, state);
                    
                    var term = VisitTerm(ctx_indexable);

                    // if term appears to be a member access
                    if (term is Skell.Types.ISkellIndexable indexable)
                        if (ctx_expression != null) {
                            Skell.Types.ISkellReturnable expression = VisitExpression(ctx_expression);
                            Source src_expression = new Source(ctx_expression.Start, ctx_expression.Stop);
                            Utility.SetIndex(
                                src,
                                indexable,
                                index,
                                Utility.GetSkellType(src_expression, expression)
                            );
                        } else if (ctx_function != null) {
                            Skell.Types.UserDefinedLambda lambda = new Skell.Types.UserDefinedLambda(ctx_function);
                            throw new Skell.Problems.UnexpectedType(
                                new Source(ctx_function.Start, ctx_function.Stop),
                                lambda,
                                typeof(Skell.Types.ISkellIndexable)
                            );
                        }
                    else
                        throw new Skell.Problems.UnexpectedType(src, term, typeof(Skell.Types.ISkellIndexable));
                }
            } else {
                throw new Skell.Problems.UnaccessibleLHS(
                    new Source(ctx_primary.Start, ctx_primary.Stop)
                );
            }

            return defaultReturnValue;
        }

        /// <summary>
        /// expression : eqExpr (KW_IS usableTypeSpecifier)? ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitExpression(SkellParser.ExpressionContext context)
        {
            var ctx_eqExpr = context.eqExpr();
            var ctx_is = context.KW_IS();

            var value = VisitEqExpr(ctx_eqExpr);
            if (ctx_is != null) {
                var ctx_uts = context.usableTypeSpecifier();

                var token = Utility.GetTokenOfUsableTypeSpecifier(ctx_uts);
                // as there is no chance of a TYPE_ANY token
                return new Skell.Types.Boolean(Utility.MatchType(value, Utility.GetSpecifier(token)));
            }
            return value;
        }

        /// <summary>
        /// eqExpr : relExpr ((OP_NE | OP_EQ) relExpr)? ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitEqExpr(SkellParser.EqExprContext context)
        {
            var ctx_curr = context.relExpr(0);
            Skell.Types.ISkellReturnable result = VisitRelExpr(ctx_curr);

            var ctx_next = context.relExpr(1);
            if (ctx_next != null) {
                Skell.Types.ISkellReturnable next = VisitRelExpr(ctx_next);
                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                bool ret = result.Equals(next);
                if (op.Type == SkellLexer.OP_NE)
                    ret = !ret;

                return new Skell.Types.Boolean(ret);
            }
            return result;
        }

        /// <summary>
        /// relExpr : addExpr ((OP_GT | OP_GE | OP_LT | OP_LE) addExpr)? ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitRelExpr(SkellParser.RelExprContext context)
        {
            var ctx_curr = context.addExpr(0);
            Skell.Types.ISkellReturnable result = VisitAddExpr(ctx_curr);

            var ctx_next = context.addExpr(1);
            if (ctx_next != null) {
                Skell.Types.ISkellReturnable next = VisitAddExpr(ctx_next);
                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (result is Skell.Types.Number r && next is Skell.Types.Number n)
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
            return result;
        }

        /// <summary>
        /// addExpr : mulExpr ((OP_SUB | OP_ADD) mulExpr)* ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitAddExpr(SkellParser.AddExprContext context)
        {
            int i = 0;
            var ctx_curr = context.mulExpr(i);
            Skell.Types.ISkellReturnable result = VisitMulExpr(ctx_curr);

            i++;
            var ctx_next = context.mulExpr(i);
            while (ctx_next != null) {
                if (!(result is Skell.Types.Number))
                    throw new Skell.Problems.UnexpectedType(
                        new Source(context.mulExpr(0).Start, ctx_curr.Stop),
                        result, typeof(Skell.Types.Number)
                    );

                Skell.Types.ISkellReturnable next = VisitMulExpr(ctx_next);
                if (!(next is Skell.Types.Number))
                    throw new Skell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        next, typeof(Skell.Types.Number)
                    );

                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_SUB) {
                    result = (Skell.Types.Number) result - (Skell.Types.Number) next;
                } else {
                    result = (Skell.Types.Number) result + (Skell.Types.Number) next;
                }

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.mulExpr(i);
            }
            return result;
        }

        /// <summary>
        /// mulExpr : unary ((OP_DIV | OP_MUL) unary)* ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitMulExpr(SkellParser.MulExprContext context)
        {
            int i = 0;
            var ctx_curr = context.unary(i);
            Skell.Types.ISkellReturnable result = VisitUnary(ctx_curr);

            i++;
            var ctx_next = context.unary(i);
            while (ctx_next != null) {
                if (!(result is Skell.Types.Number))
                    throw new Skell.Problems.UnexpectedType(
                        new Source(context.unary(0).Start, ctx_curr.Stop),
                        result, typeof(Skell.Types.Number)
                    );

                Skell.Types.ISkellReturnable next = VisitUnary(ctx_next);
                if (!(next is Skell.Types.Number))
                    throw new Skell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        result, typeof(Skell.Types.Number)
                    );

                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_DIV)
                    result = (Skell.Types.Number) result / (Skell.Types.Number) next;
                else
                    result = (Skell.Types.Number) result * (Skell.Types.Number) next;

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.unary(i);
            }
            return result;
        }

        /// <summary>
        /// control : ifControl | forControl | returnControl ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitControl(SkellParser.ControlContext context)
        {
            var ctx_if = context.ifControl();
            var ctx_for = context.forControl();
            var ctx_ret = context.returnControl();

            if (ctx_if != null)
                return VisitIfControl(ctx_if);
            else if (ctx_for != null)
                return VisitForControl(ctx_for);

            return VisitReturnControl(ctx_ret);
        }

        /// <summary>
        /// ifControl : ifThenControl | ifThenElseControl ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitIfControl(SkellParser.IfControlContext context)
        {
            var ctx_ifthen = context.ifThenControl();
            var ctx_ifthenelse = context.ifThenElseControl();

            if (ctx_ifthen != null)
                return VisitIfThenControl(ctx_ifthen);
            else
                return VisitIfThenElseControl(ctx_ifthenelse);
        }

        /// <summary>
        /// ifThenControl : KW_IF expression KW_THEN statementBlock ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitIfThenControl(SkellParser.IfThenControlContext context)
        {
            var condition = context.expression();
            var stmts = context.statementBlock();

            Skell.Types.Boolean cont = Utility.EvaluateExpr(this, condition);
            if (cont.value)
                return VisitStatementBlock(stmts);

            return defaultReturnValue;
        }

        /// <summary>
        /// ifThenElseControl : ifThenControl KW_ELSE (statementBlock | ifControl) ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitIfThenElseControl(SkellParser.IfThenElseControlContext context)
        {
            var ifthen = context.ifThenControl();
            var condition = ifthen.expression();
            var ifthenstmts = ifthen.statementBlock();
            var elsestmts = context.statementBlock();
            var elif = context.ifControl();

            // Evaluate the expression separately
            // Do not use VisitIfThenControl as the statementBlock in the
            // ifThenControl can return a false value
            Skell.Types.Boolean cont = Utility.EvaluateExpr(this, condition);
            if (cont.value)
                return VisitStatementBlock(ifthenstmts);
            else if (context.statementBlock() != null)
                return VisitStatementBlock(elsestmts);
            else
                return VisitIfControl(elif);
        }

        /// <summary>
        /// forControl : KW_FOR IDENTIFIER KW_IN expression statementBlock ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitForControl(SkellParser.ForControlContext context)
        {
            var id = context.IDENTIFIER();
            var expr = context.expression();
            var stmts = context.statementBlock();

            var primary = VisitExpression(expr);
            if (primary is Skell.Types.Property prop)
                primary = prop.value;
            if (primary is Skell.Types.Array arr) {
                string varName = id.GetText();
                Skell.Types.ISkellReturnable result = defaultReturnValue;

                state.ENTER_RETURNABLE_CONTEXT($"for {varName} in {arr}");

                foreach (Skell.Types.ISkellType data in arr) {
                    state.Variables.Set(varName, data);
                    result = VisitStatementBlock(stmts);
                    if (state.has_returned())
                    {
                        logger.Debug("For loop returned with " + result);
                        state.end_return();
                        return result;
                    }
                }

                state.EXIT_RETURNABLE_CONTEXT();
                return defaultReturnValue;
            }
            throw new Skell.Problems.UnexpectedType(
                new Source(expr.Start, expr.Stop),
                primary, typeof(Skell.Types.Array)
            );
        }

        /// <summary>
        /// returnControl : KW_RETURN expression? ;
        /// </summary>
        /// <remark>
        /// Sets FLAG_RETURN before returning
        /// </remark>
        override public Skell.Types.ISkellReturnable VisitReturnControl(SkellParser.ReturnControlContext context)
        {
            var expr = context.expression();

            if (!state.can_return()) {
                logger.Warning("Tried to return from non-returnable area");
                throw new System.NotImplementedException();
            }
            Skell.Types.ISkellReturnable retval = defaultReturnValue;
            if (expr != null)
                retval = VisitExpression(expr);
            state.start_return();
            if (retval is Skell.Types.Property prop)
                return prop.value;
            return retval;
        }

        /// <summary>
        /// unary : (OP_NOT | OP_SUB) unary
        ///       | primary
        ///       ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitUnary(SkellParser.UnaryContext context)
        {
            var unary = context.unary();
            var primary = context.primary();

            if (unary != null) {
                var op = (Antlr4.Runtime.IToken) unary.GetLeftSibling().Payload;
                if (op.Type == SkellLexer.OP_NOT) {
                    Skell.Types.Boolean boolUnary = new Skell.Types.Boolean(VisitUnary(unary).ToString());
                    return !boolUnary;
                }
                Skell.Types.Number numUnary = new Skell.Types.Number(VisitUnary(unary).ToString());
                return -numUnary;
            }
            return VisitPrimary(context.primary());
        }

        /// <summary>
        /// primary : (term | fnCall)
        ///         | LPAREN expression RPAREN
        ///         ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitPrimary(SkellParser.PrimaryContext context)
        {
            var term = context.term();
            var fnCall = context.fnCall();
            var expr = context.expression();

            if (term != null) {
                var ret = VisitTerm(term);
                // if term is a function and it is possible to call it without any arguments, call it
                if (ret is Skell.Types.Function fn) {
                    var lambda = fn.SelectLambda(new List<Tuple<int, Types.ISkellType>>());
                    if (lambda != null)
                        return Utility.Function.ExecuteFunction(
                            this, state, fn, null
                        );
                }
                return ret;
            }else if (expr != null) {
                var ret = VisitExpression(expr);
                if (ret is Skell.Types.Property prop)
                    return prop.value;
                return ret;
            } else
                return VisitFnCall(fnCall);
        }

        /// <summary>
        /// fnCall : (namespacedIdentifier | IDENTIFIER) expression+ ;
        /// </summary>
        /// <remark>
        /// If there is no argument, the call is parsed as a 'term' instead of a 'fnCall'
        /// </remark>
        override public Skell.Types.ISkellReturnable VisitFnCall(SkellParser.FnCallContext context)
        {
            var namespacedIdentifier = context.namespacedIdentifier();
            var identifier = context.IDENTIFIER();

            Skell.Types.ISkellNamedType id;
            bool isNamespaced = false;
            if (namespacedIdentifier != null) {
                id = Utility.GetNamespacedIdentifier(namespacedIdentifier, state);
                isNamespaced = true;
            } else
                id = state.Names.DefinitionOf(identifier.GetText());
            
            if (!(id is Skell.Types.Function))
                throw new Skell.Problems.UnexpectedType(
                    new Source(context.Start, context.Stop),
                    id, typeof(Skell.Types.Function)
                );

            var fn = (Skell.Types.Function) id;

            var args = new List<Tuple<int, Skell.Types.ISkellType>>();
            int i = 0;
            foreach (var arg in context.expression()) {
                var a = VisitExpression(arg);
                if (a is Skell.Types.ISkellType ar)
                    args.Add(new Tuple<int, Types.ISkellType>(i, ar));
                else
                    throw new Skell.Problems.UnexpectedType(
                        new Source(arg.Start, arg.Stop),
                        a, typeof(Skell.Types.ISkellType)
                    );

                i++;
            }

            if (isNamespaced)
                state.DISABLE_GLOBAL_FUNCTIONS(true);

            var result = Utility.Function.ExecuteFunction(
                this,
                state,
                fn,
                args
            );

            if (isNamespaced)
                state.ENABLE_GLOBAL_FUNCTIONS(true);

            return result;
        }

        /// <summary>
        /// term : value
        ///      | IDENTIFIER
        ///      | namespacedIdentifier
        ///      | term LSQR (STRING | NUMBER | IDENTIFIER) RSQR
        ///      ;
        /// </summary>
        /// <remark>
        /// term is executed if it is an identifier/namespacedIdentifier and refers to a function
        /// </remark>
        override public Skell.Types.ISkellReturnable VisitTerm(SkellParser.TermContext context)
        {
            var ctx_value = context.value();
            var ctx_term = context.term();
            var ctx_id = context.IDENTIFIER();
            var ctx_nid = context.namespacedIdentifier();

            // term : value ;
            if (ctx_value != null) {
                return VisitValue(ctx_value);
            }

            // term : IDENTIFIER ;
            if (ctx_id != null) {
                var name = context.IDENTIFIER().GetText();
                // if it is a function, it is handled in VisitPrimary()
                return Utility.GetIdentifer(new Source(ctx_id.Symbol), name, state);
            }
            
            // term : namespacedIdentifier ;
            if (ctx_nid != null) {
                var ret = Utility.GetNamespacedIdentifier(ctx_nid, state);
                // handle the function case here because VisitPrimary() will not know if
                // VisitTerm() returned a namespaced function or not
                if (ret is Skell.Types.Function func) {
                    var lambda = func.SelectLambda(new List<Tuple<int, Types.ISkellType>>());
                    if (lambda != null)
                        return Utility.Function.ExecuteNamespacedFunction(
                            this, state, func, null
                        );
                }
                return ret;
            }

            // term : term LSQR (STRING | NUMBER | IDENTIFIER) RSQR ;
            // note : could be a function call, or a member access
            var id = (Antlr4.Runtime.Tree.ITerminalNode) context.GetChild(2);
            var text = id.Symbol.Text;
            var src = new Source(id.Symbol);

            Skell.Types.ISkellType index;

            if (id.Symbol.Type == SkellLexer.STRING)
                index = Utility.GetString(text, this);
            else if (id.Symbol.Type == SkellLexer.NUMBER)
                index = new Skell.Types.Number(text);
            else 
                index = Utility.GetIdentifer(src, text, this, state);

            var term = VisitTerm(ctx_term);
            if (term is Skell.Types.Function fn) {
                // two possibilities:
                // - fn takes no arguments, returns an indexable
                // - fn takes an array/any as input
                var args = new List<Tuple<int, Skell.Types.ISkellType>>();
                if (fn.SelectLambda(args) != null) {
                    // 1st case
                    var ret = Utility.Function.ExecuteFunction(this, state, fn, args);
                    if (ret is Skell.Types.ISkellIndexable indexable1)
                        return Utility.GetIndex(src, indexable1, index);
                    else
                        throw new Skell.Problems.UnexpectedType(
                            new Source(ctx_term.Start, ctx_term.Stop),
                            ret, typeof(Skell.Types.ISkellIndexable)
                        );
                } else {
                    // 2nd case
                    args.Add(new Tuple<int, Types.ISkellType>(0, new Skell.Types.Array(index)));
                    return Utility.Function.ExecuteFunction(this, state, fn, args);
                }
            } else if (term is Skell.Types.ISkellIndexable indexable) {
                return Utility.GetIndex(src, indexable, index);
            } else {
                throw new Skell.Problems.UnexpectedType(
                    new Source(ctx_term.Start, ctx_term.Stop),
                    term, typeof(Skell.Types.ISkellIndexable)
                );
            }
        }

        /// <summary>
        /// value : object | array | STRING | NUMBER | bool ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitValue(SkellParser.ValueContext context)
        {
            var ctx_ob = context.@object();
            var ctx_ar = context.array();
            var ctx_st = context.STRING();
            var ctx_nm = context.NUMBER();
            var ctx_bl = context.@bool(); 

            if (ctx_ob != null)
                return VisitObject(ctx_ob);

            if (ctx_ar != null)
                return VisitArray(ctx_ar);

            if (ctx_st != null)
                return Utility.GetString(ctx_st.GetText(), this);

            if (ctx_nm != null)
                return new Skell.Types.Number(ctx_nm.GetText());

            return new Skell.Types.Boolean(ctx_bl.GetText());
        }

        /// <summary>
        /// array : LSQR EOL? (term (SYM_COMMA EOL? term)*)? RSQR ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitArray(SkellParser.ArrayContext context)
        {
            var values = context.term();
            var contents = new Skell.Types.ISkellType[values.Length];

            for (int i = 0; i < values.Length; i++) {
                var value = VisitTerm(values[i]);
                if (value is Skell.Types.ISkellType val)
                    contents[i] = val;
                else
                    throw new Skell.Problems.UnexpectedType(
                        new Source(values[i].Start, values[i].Stop),
                        value, typeof(Skell.Types.ISkellType)
                    );
            }
            return new Skell.Types.Array(contents);
        }

        /// <summary>
        /// object : LCURL EOL? (pair (SYM_COMMA EOL? pair)*)? RCURL ;
        /// pair : STRING SYM_COLON term ;
        /// </summary>
        override public Skell.Types.ISkellReturnable VisitObject(SkellParser.ObjectContext context)
        {
            var pairs = context.pair();
            var keys = new Skell.Types.String[pairs.Length];
            var values = new Skell.Types.ISkellType[pairs.Length];

            for (int i = 0; i < pairs.Length; i++) {
                var pair = pairs[i];
                var str = pair.STRING();
                var val = pair.term();

                keys[i] = Utility.GetString(str.GetText(), this);
                var value = VisitTerm(val);
                if (value is Skell.Types.ISkellType data)
                    values[i] = data;
                else
                    throw new Skell.Problems.UnexpectedType(
                        new Source(pair.Start, pair.Stop),
                        value, typeof(Skell.Types.ISkellType)
                    );
            }

            return new Skell.Types.Object(keys, values);
        }
    }
}