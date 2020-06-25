using Shell.Generated;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Antlr4.Runtime;
using System;

namespace Shell.Interpreter
{
    public class Visitor : ShellBaseVisitor<Shell.Types.IShellReturnable>
    {
        private readonly Shell.Types.None defaultReturnValue = new Shell.Types.None();
        private static ILogger logger;
        public ICharStream tokenSource;

        private readonly State state;

        public Visitor(State st)
        {
            logger = Log.ForContext<Visitor>();
            state = st;
        }

        /// <summary>
        /// program : program_statement+ ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitProgram(ShellParser.ProgramContext context)
        {
            var statements = context.program_statement();

            Shell.Types.IShellReturnable lastResult = defaultReturnValue;
            foreach (var statement in statements) {
                lastResult = VisitProgram_statement(statement);
                if (lastResult is Shell.Types.String str)
                    logger.Debug($"Result: \"{lastResult}\" of type {lastResult.GetType()}");
                else if (lastResult is Shell.Types.Array arr)
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()} and length {arr.Count()}");
                else if (lastResult is Shell.Types.Object obj)
                    logger.Debug($"Result is an object:\n {obj}");
                else if (!(lastResult is Shell.Types.None))
                    logger.Debug($"Result: {lastResult} of type {lastResult.GetType()}");
            }
            return lastResult;
        }

        /// <summary>
        /// program_statement : namespace_declaration EOL
        ///                   | statement
        ///                   ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitProgram_statement(ShellParser.Program_statementContext context)
        {
            var ctx_ns = context.namespace_declaration();
            var ctx_stmt = context.statement();

            if (ctx_ns != null) {
                var ns = new Shell.Types.Namespace("", ".", ctx_ns, this);
                state.Namespaces.Register(ns);
                return defaultReturnValue;
            } else
                return VisitStatement(ctx_stmt);
        }

        /// <summary>
        /// statement : EOL
        ///           | load_namespace EOL
        ///           | program_shorthand EOL
        ///           | declaration EOL
        ///           | expression EOL
        ///           | control
        ///           ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitStatement(ShellParser.StatementContext context)
        {
            var ctx_nsLoad = context.load_namespace();
            var ctx_progExec = context.program_shorthand();
            var ctx_decl = context.declaration();
            var ctx_expr = context.expression();
            var ctx_ctrl = context.control();

            if (ctx_nsLoad != null)
                return VisitLoad_namespace(ctx_nsLoad);
            else if (ctx_progExec != null)
                return VisitProgram_shorthand(ctx_progExec);
            else if (ctx_decl != null)
                return VisitDeclaration(ctx_decl);
            else if (ctx_expr != null) {
                var ret = VisitExpression(ctx_expr);
                ret = Utility.GetReturnableValue(ret);
                return ret;
            } else if (ctx_ctrl != null)
                return VisitControl(ctx_ctrl);

            return defaultReturnValue;
        }

        /// <summary>
        /// load_namespace : KW_USING STRING (KW_AS IDENTIFIER)? ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitLoad_namespace(ShellParser.Load_namespaceContext context)
        {
            var ctx_str = context.STRING();
            var ctx_id = context.IDENTIFIER();

            var name = Utility.GetString(ctx_str.GetText(), this);
            var ns = Utility.LoadNamespace(
                    name.contents,
                    ctx_id != null ? ctx_id.GetText() : "",
                    this
                );
            if (state.Names.DefinedAs(ns.name, typeof(Shell.Types.Namespace)))
                logger.Debug($"Overwriting namespace {ns.name}");

            state.Namespaces.Register(ns);
            return defaultReturnValue;
        }

        /// <summary>
        /// program_shorthand : SYM_DOLLAR ~EOL* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitProgram_shorthand(ShellParser.Program_shorthandContext context)
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
                        return str[1..^1];

                    return str;
                }
            ).ToList();

            logger.Information($"Running command: {execString}");
            process.StartInfo.Arguments = string.Join(" ", argsList);
            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            logger.Information($"Process {process.StartInfo.FileName} exited with return code {exitCode}");

            return new Shell.Types.Number(exitCode);
        }

        /// <summary>
        /// statement_block : LCURL statement* RCURL ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitStatement_block(ShellParser.Statement_blockContext context)
        {
            var statements = context.statement();
            foreach (var statement in statements) {
                var result = VisitStatement(statement);
                if (state.HasReturned())
                    // do not unset state.has_returned() here as it is the caller's job
                    return result;
            }
            return defaultReturnValue;
        }

        /// <summary>
        /// declaration : KW_LET IDENTIFIER (LSQR expression RSQR)* OP_ASSGN (expression | function);
        /// function : KW_FUN LPAREN (function_argument (SYM_COMMA function_argument)*)? RPAREN statement_block ;
        /// function_argument : type_specifier IDENTIFIER ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitDeclaration(ShellParser.DeclarationContext context)
        {
            ShellParser.ExpressionContext ctx_expression = null;
            // if there are no expressions context.expression() is not null but an empty array
            if (context.expression().Count() != 0)
                ctx_expression = context.expression().Last();
            var indices = context.expression().SkipLast(1);
            var ctx_function = context.function();
            var ctx_id = context.IDENTIFIER();
            string name = ctx_id.GetText();

            var isMember = context.LSQR().Length != 0;

            if (isMember) {
                if (state.Names.Available(name))
                    throw new Shell.Problems.UndefinedIdentifer(new Source(ctx_id.Symbol), name);
                
                if (ctx_expression == null)
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_expression.Start, ctx_expression.Stop),
                        new Shell.Types.UserDefinedLambda(ctx_function),
                        typeof(Shell.Types.IShellData)
                    );

                Shell.Types.IShellIndexable current;
                // first ensure that identifier is referring to an indexable
                if (state.Names.DefinedAsIndexable(name)) {
                    current = (Shell.Types.IShellIndexable) state.Names.DefinitionOf(name);

                    foreach (var index in indices.SkipLast(1)) {
                        var result = Utility.GetIndex(
                            new Source(index.Start, index.Stop),
                            current,
                            (Shell.Types.IShellData) VisitExpression(index)
                        );
                        
                        if (!(result is Shell.Types.IShellIndexable))
                            throw new Shell.Problems.UnexpectedType(
                                new Source(ctx_id.Symbol, index.Stop),
                                result,
                                typeof(Shell.Types.IShellIndexable)
                            );
                        
                        current = (Shell.Types.IShellIndexable) result;
                    }

                    var lastIndex = (Shell.Types.IShellData) VisitExpression(indices.Last());
                    if (current.ListIndices().Contains(lastIndex))
                        current.Replace(lastIndex, Utility.GetShellType(
                            new Source(ctx_id.Symbol, indices.Last().Stop),
                            VisitExpression(ctx_expression)
                        ));
                    else
                        throw new Shell.Problems.IndexOutOfRange(
                            new Source(indices.Last().Start, indices.Last().Stop),
                            lastIndex, current
                        );
                } else
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_id.Symbol),
                        state.Names.DefinitionOf(name),
                        typeof(Shell.Types.IShellIndexable)
                    );
            } else {
                if (ctx_expression != null) {
                    var value = VisitExpression(ctx_expression);
                    value = Utility.GetReturnableValue(value);
                    
                    Source src_expression = new Source(ctx_expression.Start, ctx_expression.Stop);

                    if (state.Names.Available(name) || state.Names.DefinedAsVariable(name))
                        state.Variables.Set(name, Utility.GetShellType(src_expression, value));
                    else
                        throw new Shell.Problems.InvalidDefinition(src_expression, name, state);
                } else {
                    Source src_function = new Source(ctx_function.Start, ctx_function.Stop);

                    if (state.Names.Available(name) || state.Names.DefinedAs(name, typeof(Shell.Types.Function))) {
                        var fn = Utility.Function.Get(state, name);
                        fn.AddUserDefinedLambda(ctx_function);
                    } else
                        throw new Shell.Problems.InvalidDefinition(src_function, name, state);
                }
            }

            return defaultReturnValue;
        }

        /// <summary>
        /// expression : expression_or (KW_IS value_type_specifier)? | function_call ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression(ShellParser.ExpressionContext context)
        {
            var ctx_expression_or = context.expression_or();
            var ctx_is = context.KW_IS();
            var ctx_function_call = context.function_call();

            if (ctx_expression_or != null) {
                var value = VisitExpression_or(ctx_expression_or);
                if (ctx_is != null) {
                    var ctx_uts = context.value_type_specifier();

                    var token = Utility.GetTokenOfValueTypeSpecifier(ctx_uts);
                    // as there is no chance of a TYPE_ANY token
                    return new Shell.Types.Boolean(Utility.MatchType(value, Utility.GetSpecifier(token)));
                }
                return value;
            } else {
                return VisitFunction_call(ctx_function_call);
            }
        }

        /// <summary>
        /// expression_or : expression_and (OP_OR expression_and)* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_or(ShellParser.Expression_orContext context)
        {
            int i = 0;
            var ctx_curr = context.expression_and(i);
            Shell.Types.IShellReturnable result = VisitExpression_and(ctx_curr);

            i++;
            var ctx_next = context.expression_and(i);
            while (ctx_next != null) {
                if (!(result is Shell.Types.Boolean))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(context.expression_and(0).Start, ctx_curr.Stop),
                        result, typeof(Shell.Types.Boolean)
                    );
                
                var v1 = (Shell.Types.Boolean) result;
                // short circuit
                if (v1.value)
                    return v1;

                Shell.Types.IShellReturnable next = VisitExpression_and(ctx_next);
                if (!(next is Shell.Types.Boolean))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        next, typeof(Shell.Types.Boolean)
                    );

                var v2 = (Shell.Types.Boolean) next;
                result = new Shell.Types.Boolean(v1.value || v2.value);

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.expression_and(i);
            }
            return result;
        }

        /// <summary>
        /// expression_and : expression_equality (OP_AND expression_equality)* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_and(ShellParser.Expression_andContext context)
        {
            int i = 0;
            var ctx_curr = context.expression_equality(i);
            Shell.Types.IShellReturnable result = VisitExpression_equality(ctx_curr);

            i++;
            var ctx_next = context.expression_equality(i);
            while (ctx_next != null) {
                if (!(result is Shell.Types.Boolean))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(context.expression_equality(0).Start, ctx_curr.Stop),
                        result, typeof(Shell.Types.Boolean)
                    );

                var v1 = (Shell.Types.Boolean) result;
                // short circuit
                if (!v1.value)
                    return v1;

                Shell.Types.IShellReturnable next = VisitExpression_equality(ctx_next);
                if (!(next is Shell.Types.Boolean))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        next, typeof(Shell.Types.Boolean)
                    );

                var v2 = (Shell.Types.Boolean) next;
                result = new Shell.Types.Boolean(v1.value && v2.value);

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.expression_equality(i);
            }
            return result;
        }

        /// <summary>
        /// expression_equality : expression_relational ((OP_NE | OP_EQ) expression_relational)? ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_equality(ShellParser.Expression_equalityContext context)
        {
            var ctx_curr = context.expression_relational(0);
            Shell.Types.IShellReturnable result = VisitExpression_relational(ctx_curr);

            var ctx_next = context.expression_relational(1);
            if (ctx_next != null) {
                Shell.Types.IShellReturnable next = VisitExpression_relational(ctx_next);
                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                bool ret = result.Equals(next);
                if (op.Type == ShellLexer.OP_NE)
                    ret = !ret;

                return new Shell.Types.Boolean(ret);
            }
            return result;
        }

        /// <summary>
        /// expression_relational : expression_additive ((OP_GT | OP_GE | OP_LT | OP_LE) expression_additive)? ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_relational(ShellParser.Expression_relationalContext context)
        {
            var ctx_curr = context.expression_additive(0);
            Shell.Types.IShellReturnable result = VisitExpression_additive(ctx_curr);

            var ctx_next = context.expression_additive(1);
            if (ctx_next != null) {
                Shell.Types.IShellReturnable next = VisitExpression_additive(ctx_next);
                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (result is Shell.Types.Number r && next is Shell.Types.Number n)
                    switch (op.Type) {
                        case ShellLexer.OP_GE:
                            return r >= n;
                        case ShellLexer.OP_GT:
                            return r > n;
                        case ShellLexer.OP_LE:
                            return r <= n;
                        case ShellLexer.OP_LT:
                            return r < n;
                    }
            }
            return result;
        }

        /// <summary>
        /// expression_additive : expression_multiplicative ((OP_SUB | OP_ADD) expression_multiplicative)* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_additive(ShellParser.Expression_additiveContext context)
        {
            int i = 0;
            var ctx_curr = context.expression_multiplicative(i);
            Shell.Types.IShellReturnable result = VisitExpression_multiplicative(ctx_curr);

            i++;
            var ctx_next = context.expression_multiplicative(i);
            while (ctx_next != null) {
                if (!(result is Shell.Types.Number))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(context.expression_multiplicative(0).Start, ctx_curr.Stop),
                        result, typeof(Shell.Types.Number)
                    );

                Shell.Types.IShellReturnable next = VisitExpression_multiplicative(ctx_next);
                if (!(next is Shell.Types.Number))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        next, typeof(Shell.Types.Number)
                    );

                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (op.Type == ShellLexer.OP_SUB) {
                    result = (Shell.Types.Number) result - (Shell.Types.Number) next;
                } else {
                    result = (Shell.Types.Number) result + (Shell.Types.Number) next;
                }

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.expression_multiplicative(i);
            }
            return result;
        }

        /// <summary>
        /// expression_multiplicative : expression_unary ((OP_DIV | OP_MUL | OP_MOD) expression_unary)* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_multiplicative(ShellParser.Expression_multiplicativeContext context)
        {
            int i = 0;
            var ctx_curr = context.expression_unary(i);
            Shell.Types.IShellReturnable result = VisitExpression_unary(ctx_curr);

            i++;
            var ctx_next = context.expression_unary(i);
            while (ctx_next != null) {
                if (!(result is Shell.Types.Number))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(context.expression_unary(0).Start, ctx_curr.Stop),
                        result, typeof(Shell.Types.Number)
                    );

                Shell.Types.IShellReturnable next = VisitExpression_unary(ctx_next);
                if (!(next is Shell.Types.Number))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(ctx_next.Start, ctx_next.Stop),
                        result, typeof(Shell.Types.Number)
                    );

                var op = (Antlr4.Runtime.IToken) ctx_next.GetLeftSibling().Payload;
                if (op.Type == ShellLexer.OP_DIV)
                    try {
                        result = (Shell.Types.Number) result / (Shell.Types.Number) next;
                    } catch (System.DivideByZeroException) {
                        throw new Shell.Problems.DivisionByZero(new Source(op));
                    }
                else if (op.Type == ShellLexer.OP_MUL)
                    result = (Shell.Types.Number) result * (Shell.Types.Number) next;
                else
                    result = (Shell.Types.Number) result % (Shell.Types.Number) next;

                i++;
                ctx_curr = ctx_next;
                ctx_next = context.expression_unary(i);
            }
            return result;
        }

        /// <summary>
        /// expression_unary : (OP_NOT | OP_SUB) expression_unary
        ///                  | expression_primary
        ///                  ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_unary(ShellParser.Expression_unaryContext context)
        {
            var expression_unary = context.expression_unary();
            var expression_primary = context.expression_primary();

            if (expression_unary != null) {
                var op = (Antlr4.Runtime.IToken) expression_unary.GetLeftSibling().Payload;
                if (op.Type == ShellLexer.OP_NOT) {
                    Shell.Types.Boolean boolExpression_unary = new Shell.Types.Boolean(VisitExpression_unary(expression_unary).ToString());
                    return !boolExpression_unary;
                }
                Shell.Types.Number numExpression_unary = new Shell.Types.Number(VisitExpression_unary(expression_unary).ToString());
                return -numExpression_unary;
            }
            return VisitExpression_primary(expression_primary);
        }

        /// <summary>
        /// expression_primary : term
        ///                    | LPAREN expression RPAREN
        ///                    ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitExpression_primary(ShellParser.Expression_primaryContext context)
        {
            var term = context.term();
            var expr = context.expression();

            if (term != null) {
                var ret = VisitTerm(term);
                // if term is a function and it is possible to call it without any arguments, call it
                if (ret is Shell.Types.Function fn) {
                    var lambda = fn.SelectLambda(new List<Tuple<int, Types.IShellData>>());
                    if (lambda != null)
                        return Utility.Function.ExecuteFunction(
                            this, state, fn, null
                        );
                }
                return ret;
            } else {
                var ret = VisitExpression(expr);
                ret = Utility.GetReturnableValue(ret);
                return ret;
            }
        }

        /// <summary>
        /// control : control_if | control_for | control_return ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitControl(ShellParser.ControlContext context)
        {
            var ctx_if = context.control_if();
            var ctx_for = context.control_for();
            var ctx_ret = context.control_return();

            if (ctx_if != null)
                return VisitControl_if(ctx_if);
            else if (ctx_for != null)
                return VisitControl_for(ctx_for);

            return VisitControl_return(ctx_ret);
        }

        /// <summary>
        /// control_if : if_then | if_then_else ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitControl_if(ShellParser.Control_ifContext context)
        {
            var ctx_ifthen = context.if_then();
            var ctx_ifthenelse = context.if_then_else();

            if (ctx_ifthen != null)
                return VisitIf_then(ctx_ifthen);
            else
                return VisitIf_then_else(ctx_ifthenelse);
        }

        /// <summary>
        /// if_then : KW_IF expression statement_block ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitIf_then(ShellParser.If_thenContext context)
        {
            var condition = context.expression();
            var stmts = context.statement_block();

            Shell.Types.Boolean cont = Utility.EvaluateExpr(this, condition);
            if (cont.value)
                return VisitStatement_block(stmts);

            return defaultReturnValue;
        }

        /// <summary>
        /// if_then_else : if_then KW_ELSE (statement_block | control_if) ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitIf_then_else(ShellParser.If_then_elseContext context)
        {
            var ifthen = context.if_then();
            var condition = ifthen.expression();
            var ifthenstmts = ifthen.statement_block();
            var elsestmts = context.statement_block();
            var elif = context.control_if();

            // Evaluate the situation manually
            // Do not use VisitIf_then as it does not return values
            // based on whether the condition is true or false
            Shell.Types.Boolean cont = Utility.EvaluateExpr(this, condition);
            if (cont.value)
                return VisitStatement_block(ifthenstmts);
            else if (elsestmts != null)
                return VisitStatement_block(elsestmts);
            else
                return VisitControl_if(elif);
        }

        /// <summary>
        /// control_for : KW_FOR IDENTIFIER KW_IN expression statement_block ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitControl_for(ShellParser.Control_forContext context)
        {
            var id = context.IDENTIFIER();
            var expr = context.expression();
            var stmts = context.statement_block();

            var expression_primary = VisitExpression(expr);
            expression_primary = Utility.GetReturnableValue(expression_primary);
            if (expression_primary is Shell.Types.Array arr) {
                string varName = id.GetText();

                // Shadow the name if it already exists
                Shell.Types.IShellNamed backup = null;
                if (state.Names.Exists(varName)) {
                    backup = state.Names.DefinitionOf(varName);
                    state.Names.Clear(varName);
                }

                Shell.Types.IShellReturnable returnvalue = defaultReturnValue;

                for (int i = 0; i < arr.Count().integerValue; i++) {
                    var data = arr.GetMember(new Shell.Types.Number(i));
                    state.Variables.Set(varName, data);
                    var ret = VisitStatement_block(stmts);
                    if (state.HasReturned()) {
                        returnvalue = ret;
                        break;
                    }
                }

                // restore the name if it existed prior to the loop
                if (backup != null) {
                    if (backup is Shell.Types.Namespace ns)
                        state.Namespaces.Set(varName, ns);
                    else if (backup is Shell.Types.Function fn)
                        state.Functions.Set(varName, fn);
                    else
                        state.Variables.Set(varName, (Shell.Types.IShellData) backup);
                }

                return returnvalue;
            }
            throw new Shell.Problems.UnexpectedType(
                new Source(expr.Start, expr.Stop),
                expression_primary, typeof(Shell.Types.Array)
            );
        }

        /// <summary>
        /// control_return : KW_RETURN expression? ;
        /// </summary>
        /// <remark>
        /// Sets FLAG_RETURN before returning
        /// </remark>
        override public Shell.Types.IShellReturnable VisitControl_return(ShellParser.Control_returnContext context)
        {
            var expr = context.expression();

            if (!state.CanReturn())
                throw new Shell.Problems.InvalidReturn(new Source(context.Start, context.Stop));
            
            Shell.Types.IShellReturnable retval = new Shell.Types.Null();
            if (expr != null)
                retval = VisitExpression(expr);
            state.StartReturn();
            retval = Utility.GetReturnableValue(retval);
            return retval;
        }

        /// <summary>
        /// function_call : (identifier_namespaced | IDENTIFIER) expression* ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitFunction_call(ShellParser.Function_callContext context)
        {
            var identifier_namespaced = context.identifier_namespaced();
            var identifier = context.IDENTIFIER();

            Shell.Types.IShellNamed id;
            if (identifier_namespaced != null)
                id = Utility.GetNamespacedIdentifier(identifier_namespaced, state);
            else
                id = state.Names.DefinitionOf(identifier.GetText());
            
            if (!(id is Shell.Types.Function))
                throw new Shell.Problems.UnexpectedType(
                    new Source(context.Start, context.Stop),
                    id, typeof(Shell.Types.Function)
                );

            var fn = (Shell.Types.Function) id;

            var args = new List<Tuple<int, Shell.Types.IShellData>>();
            int i = 0;
            foreach (var arg in context.expression()) {
                var a = VisitExpression(arg);
                if (a is Shell.Types.IShellData ar)
                    args.Add(new Tuple<int, Types.IShellData>(i, ar));
                else
                    throw new Shell.Problems.UnexpectedType(
                        new Source(arg.Start, arg.Stop),
                        a, typeof(Shell.Types.IShellData)
                    );

                i++;
            }

            var result = Utility.Function.ExecuteFunction(
                this,
                state,
                fn,
                args
            );

            return result;
        }

        /// <summary>
        /// term : value
        ///      | IDENTIFIER
        ///      | identifier_namespaced
        ///      | term LSQR expression RSQR
        ///      ;
        /// </summary>
        /// <remark>
        /// term is executed if it is an identifier/identifier_namespaced and refers to a function
        /// </remark>
        override public Shell.Types.IShellReturnable VisitTerm(ShellParser.TermContext context)
        {
            var ctx_value = context.value();
            var ctx_term = context.term();
            var ctx_id = context.IDENTIFIER();
            var ctx_nid = context.identifier_namespaced();
            var ctx_index = context.expression();

            // term : value ;
            if (ctx_value != null) {
                return VisitValue(ctx_value);
            }

            // term : IDENTIFIER ;
            if (ctx_id != null) {
                var name = context.IDENTIFIER().GetText();
                return Utility.GetIdentifer(new Source(ctx_id.Symbol), name, state);
            }
            
            // term : identifier_namespaced ;
            if (ctx_nid != null) {
                return Utility.GetNamespacedIdentifier(ctx_nid, state);
            }

            // term : term LSQR index RSQR ;
            // note : could be a function call, or a member access
            var src = new Source(ctx_index.Start, ctx_index.Stop);

            var index = (Shell.Types.IShellData) VisitExpression(ctx_index);

            var term = VisitTerm(ctx_term);
            if (term is Shell.Types.Function fn) {
                // two possibilities:
                // - fn takes no arguments, returns an indexable
                // - fn takes an array/any as input

                var args = new List<Tuple<int, Shell.Types.IShellData>>();
                if (fn.SelectLambda(args) != null) {
                    // 1st case
                    var ret = Utility.Function.ExecuteFunction(this, state, fn, args);
                    if (ret is Shell.Types.IShellIndexable indexable1)
                        return Utility.GetIndex(src, indexable1, index);
                    else
                        throw new Shell.Problems.UnexpectedType(
                            new Source(ctx_term.Start, ctx_term.Stop),
                            ret, typeof(Shell.Types.IShellIndexable)
                        );
                } else {
                    // 2nd case
                    args.Add(new Tuple<int, Types.IShellData>(0, new Shell.Types.Array(index)));
                    return Utility.Function.ExecuteFunction(this, state, fn, args);
                }
            } else if (term is Shell.Types.IShellIndexable indexable) {
                return Utility.GetIndex(src, indexable, index);
            } else {
                throw new Shell.Problems.UnexpectedType(
                    new Source(ctx_term.Start, ctx_term.Stop),
                    term, typeof(Shell.Types.IShellIndexable)
                );
            }
        }

        /// <summary>
        /// value : object | array | STRING | NUMBER | bool | TYPE_NULL ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitValue(ShellParser.ValueContext context)
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
                return new Shell.Types.Number(ctx_nm.GetText());

            if  (ctx_bl != null)
                return new Shell.Types.Boolean(ctx_bl.GetText());
            
            return new Shell.Types.Null();
        }

        /// <summary>
        /// array : LSQR EOL? (term (SYM_COMMA EOL? term)*)? RSQR ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitArray(ShellParser.ArrayContext context)
        {
            var values = context.term();
            var contents = new Shell.Types.IShellData[values.Length];

            for (int i = 0; i < values.Length; i++) {
                var value = VisitTerm(values[i]);
                if (value is Shell.Types.IShellData val)
                    contents[i] = val;
                else
                    throw new Shell.Problems.UnexpectedType(
                        new Source(values[i].Start, values[i].Stop),
                        value, typeof(Shell.Types.IShellData)
                    );
            }
            return new Shell.Types.Array(contents);
        }

        /// <summary>
        /// object : LCURL EOL? (pair (SYM_COMMA EOL? pair)*)? RCURL ;
        /// pair : STRING SYM_COLON term ;
        /// </summary>
        override public Shell.Types.IShellReturnable VisitObject(ShellParser.ObjectContext context)
        {
            var pairs = context.pair();
            var keys = new Shell.Types.String[pairs.Length];
            var values = new Shell.Types.IShellData[pairs.Length];

            for (int i = 0; i < pairs.Length; i++) {
                var pair = pairs[i];
                var str = pair.STRING();
                var val = pair.term();

                keys[i] = Utility.GetString(str.GetText(), this);
                var value = VisitTerm(val);
                if (value is Shell.Types.IShellData data)
                    values[i] = data;
                else
                    throw new Shell.Problems.UnexpectedType(
                        new Source(pair.Start, pair.Stop),
                        value, typeof(Shell.Types.IShellData)
                    );
            }

            return new Shell.Types.Object(keys, values);
        }
    }
}