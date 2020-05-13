using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Serilog;
using Shell.Generated;

namespace Shell.Interpreter
{
    internal static class Utility
    {
        public static class Function
        {
            private static readonly ILogger logger = Log.ForContext(typeof(Function));

            /// <summary>
            /// Retrieves the function, creating a new one if necessary
            /// </summary>
            public static Shell.Types.Function Get(State state, string name)
            {
                Shell.Types.Function fn;
                if (state.Names.Exists(name) && state.Names.DefinedAs(name, typeof(Shell.Types.Function))) {
                    fn = state.Functions.Get(name);
                    return fn;
                }
                fn = new Shell.Types.Function(name);
                state.Functions.Set(name, fn);
                return fn;
            }

            /// <summary>
            /// Sets up, executes, and then cleans up function calls.
            /// </summary>
            /// <remark>
            /// Set name to "" if the function is namespaced.
            /// </remark>
            public static Shell.Types.IShellReturnable ExecuteFunction(
                ShellBaseVisitor<Shell.Types.IShellReturnable> visitor,
                State state,
                Shell.Types.Function function,
                List<Tuple<int, Shell.Types.IShellData>> unnamedArgs
            )
            {
                Shell.Types.IShellReturnable returnValue;
                if (unnamedArgs == null)
                    unnamedArgs = new List<Tuple<int, Types.IShellData>>();

                var lambda = function.SelectLambda(unnamedArgs);
                if (lambda == null)
                    throw new Shell.Problems.InvalidFunctionCall(function, unnamedArgs);
                var args = lambda.NameArguments(unnamedArgs);

                string argString = "";
                if (args.Count > 0) {
                    var sb = new StringBuilder(" ");
                    for (int i = 0; i < args.Count; i++) {
                        sb.Append($"{args[i].Item2}:{args[i].Item3}");
                        if (i + 1 != args.Count)
                            sb.Append(" ");
                    }
                    argString = sb.ToString();
                }
                
                // pre-setup for state
                state.ENTER_CONTEXT($"{function.name}{argString}");

                // allow the function to call itself through its own identifier
                state.Functions.Set(function.name, function);

                // set up state with arguments in context
                foreach (var arg in args)
                    if (arg.Item3 is Shell.Types.Property prop)
                        if (lambda is Shell.Types.BuiltinLambda)
                            state.Variables.Set(arg.Item2, prop);
                        else
                            state.Variables.Set(arg.Item2, prop.Value);
                    else 
                        state.Variables.Set(arg.Item2, arg.Item3);
                
                if (lambda is Shell.Types.UserDefinedLambda udLambda)
                    returnValue = udLambda.Execute(visitor);
                else
                    returnValue = ((Shell.Types.BuiltinLambda) lambda).Execute(state, args);

                //cleanup
                if (state.HasReturned()) {
                    logger.Debug($"{function.name} returned {returnValue}");
                    state.EndReturn();
                }
                state.EXIT_CONTEXT();

                return returnValue;
            }
        }

        /// <summary>
        /// Loads a namespace definition from file
        /// </summary>
        /// <remark>
        /// Pass rename="" to load namespace with original name
        /// </remark>
        public static Shell.Types.Namespace LoadNamespace(string path, string rename, Visitor visitor)
        {
            var pathFull = Path.GetFullPath(path);
            var pathDir = Path.GetDirectoryName(pathFull);
            // Read and prepare file
            var src = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            if (src.Last() != '\n')
                src += '\n';

            var charStream = CharStreams.fromstring(src);
            var lexer = new ShellLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ShellParser(tokenStream);
            var tree = parser.namespace_declaration();

            return new Shell.Types.Namespace(rename, pathDir, tree, visitor);
        }

        /// <summary>
        /// Safely set the value of an indexed variable with an index
        /// </summary>
        /// <remark>
        /// source should refer to the index' source
        /// </remark>
        public static void SetIndex(
            Source source,
            Shell.Types.IShellIndexable indexable,
            Shell.Types.IShellData index,
            Shell.Types.IShellData value
        )
        {
            if (indexable is Shell.Types.Array)
                if (index is Shell.Types.Number num && num.isInt)
                    indexable.Replace(index, value);
                else
                    throw new Shell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Shell.Types.Number)
                    );
            else
                if (index is Shell.Types.String)
                    indexable.Replace(index, value);
                else
                    throw new Shell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Shell.Types.String)
                    );
        }

        /// <summary>
        /// Safely index into an indexable object, checking types
        /// Source should be source of index
        /// </summary>
        public static Shell.Types.IShellData GetIndex(
            Source source,
            Shell.Types.IShellIndexable indexable,
            Shell.Types.IShellData index
        )
        {
            if (indexable is Shell.Types.Array)
                if (!(index is Shell.Types.Number num && num.isInt))
                    throw new Shell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Shell.Types.Number)
                    );
            else if (indexable is Shell.Types.Object)
                if (!(index is Shell.Types.String))
                    throw new Shell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Shell.Types.String)
                    );

            if (indexable.ListIndices().Contains(index))
                return indexable.GetMember(index);
            else
                throw new Shell.Problems.IndexOutOfRange(source, index, indexable);
        }

        /// <summary>
        /// Returns the value pointed to by the name, without calling it
        /// </summary>
        public static Shell.Types.IShellNamed GetIdentifer(
            Source src, string name,
            State state
        )
        {
            if (state.Names.Available(name))
                throw new Shell.Problems.UndefinedIdentifer(
                    src,
                    name
                );
                    
            return state.Names.DefinitionOf(name);
        }

        /// <summary>
        /// Wrapper over GetIdentifier, calling the returned value with no args if it is a function
        /// </summary>
        public static Shell.Types.IShellData GetIdentifer(
            Source src, string name,
            Visitor visitor, State state
        )
        {
            var named = Utility.GetIdentifer(src, name, state);
            if (named is Shell.Types.Function fn) {
                var result = Utility.Function.ExecuteFunction(
                        visitor,
                        state,
                        fn,
                        null
                    );
                return Utility.GetShellType(src, result);
            } else 
                throw new Shell.Problems.UnexpectedType(
                    src, named,
                    typeof(Shell.Types.IShellData),
                    typeof(Shell.Types.Function)
                );
            
        }

        /// <summary>
        /// Small wrapper over Utility.GetNamespacedIdentifier to call the return value if it is a function
        /// </summary>
        public static Shell.Types.IShellReturnable GetNamespacedIdentifier(
            ShellParser.Identifier_namespacedContext context,
            Visitor visitor,
            State state
        )
        {
            var named = Utility.GetNamespacedIdentifier(context, state);

            if (named is Shell.Types.Function fn)
                return Utility.Function.ExecuteFunction(
                    visitor,
                    state,
                    fn,
                    null
                );
            
            return named;
        }

        /// <summary>
        /// Error out if the returnable is None
        /// </summary>
        public static Shell.Types.IShellData GetShellType(Source src, Shell.Types.IShellReturnable returnable)
        {
            if (returnable is Shell.Types.IShellData rdata)
                return rdata;
            else
                throw new Shell.Problems.UnexpectedType(
                    src,
                    returnable,
                    typeof(Shell.Types.IShellData)
                );
        }

        /// <summary>
        /// namespacedIdentifier : (IDENTIFIER SYM_PERIOD)+ IDENTIFIER ;
        /// </summary>
        public static Shell.Types.IShellNamed GetNamespacedIdentifier(
            ShellParser.Identifier_namespacedContext context,
            State state
        )
        {
            var rootName = context.IDENTIFIER().First().GetText();

            if (state.Names.Available(rootName))
                throw new Shell.Problems.UndefinedIdentifer(
                    new Source(context.IDENTIFIER().First().Symbol),
                    rootName,
                    typeof(Shell.Types.Namespace)
                );

            var nst = state.Names.DefinitionOf(rootName);
            if (!(nst is Shell.Types.Namespace))
                throw new Shell.Problems.UnexpectedType(
                    new Source(context.IDENTIFIER().First().Symbol),
                    nst,
                    typeof(Shell.Types.Namespace)
                );

            var ns = (Shell.Types.Namespace) nst;
            var nsRoot = ns;
            foreach (var id in context.IDENTIFIER().Skip(1).SkipLast(1)) {
                nst = ns.Get(id.GetText());
                if (!(nst is Shell.Types.Namespace))
                    throw new Shell.Problems.UnexpectedType(
                        new Source(context.IDENTIFIER().First().Symbol),
                        nst,
                        typeof(Shell.Types.Namespace)
                    );

                ns = (Shell.Types.Namespace) nst;
            }
            
            var name = context.IDENTIFIER().Last().GetText();
            if (ns.Exists(name)) {
                var result = ns.Get(context.IDENTIFIER().Last().GetText());
                return result;
            } else
                throw new Shell.Problems.InvalidNamespacedIdentifier(
                    new Source(context.IDENTIFIER().First().Symbol, context.IDENTIFIER().Last().Symbol),
                    new Shell.Types.Array(nsRoot.ListNames())
                );
        }

        /// <summary>
        /// Returns a Shell.Types.String for a STRING token's text contents.
        /// Requires visitor to handle string substitution
        /// </summary>
        /// <remark>
        /// STRING : SYM_QUOTE (ESC | SAFECODEPOINT | SYM_DOLLAR LCURL ~[}]* RCURL)* SYM_QUOTE ;
        /// </remark>
        public static Shell.Types.String GetString(
            string contents,
            Visitor visitor
        )
        {
            contents = contents.Remove(0, 1);
            contents = contents.Remove(contents.Length - 1, 1);
            contents = Regex.Replace(
                contents,
                @"\$\{[^\$\{\}]*\}",
                m => {
                    string src = m.Value.Remove(0, 2);
                    src = src.Remove(src.Length - 1, 1);
                        
                    var charStream = CharStreams.fromstring(src);
                    var lexer = new ShellLexer(charStream);
                    var tokenStream = new CommonTokenStream(lexer);
                    var parser = new ShellParser(tokenStream);
                    var tree = parser.expression();

                    var dupSrc = visitor.tokenSource;
                    visitor.tokenSource = charStream;
                    var value = visitor.VisitExpression(tree);
                    visitor.tokenSource = dupSrc;

                    if (value is Shell.Types.String str)
                        return str.contents;
                    else if (value is Shell.Types.IShellData)
                        return value.ToString();
                    else
                        return "";
                }
            );
            return new Shell.Types.String(contents);
        }

        /// <summary>
        /// Returns the token for the given type_specifier context
        /// </summary>
        /// <remark>
        /// type_specifier : type_specifier_value | TYPE_ANY ;
        /// type_specifier_value : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTokenOfTypeSpecifier(ShellParser.Type_specifierContext context)
        {
            if (context.type_specifier_value() != null)
                return Utility.GetTokenOfValueTypeSpecifier(context.type_specifier_value());
            
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }
        
        /// <summary>
        /// Returns the token for the given type_specifier_value context
        /// </summary>
        /// <remark>
        /// type_specifier : type_specifier_value | TYPE_ANY ;
        /// type_specifier_value : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTokenOfValueTypeSpecifier(ShellParser.Type_specifier_valueContext context)
        {
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }

        /// <summary>
        /// Returns true if the data matches the given type specifier
        /// </summary>
        public static bool MatchType(Shell.Types.IShellInternal data, Shell.Types.Specifier type)
        {
            if (data is Shell.Types.Array && type == Shell.Types.Specifier.Array)
                return true;
            else if (data is Shell.Types.Boolean && type == Shell.Types.Specifier.Bool)
                return true;
            else if (data is Shell.Types.Number && type == Shell.Types.Specifier.Number)
                return true;
            else if (data is Shell.Types.Object && type == Shell.Types.Specifier.Object)
                return true;
            else if (data is Shell.Types.String && type == Shell.Types.Specifier.String)
                return true;
            else if (data is Shell.Types.IShellData && type == Shell.Types.Specifier.Any)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the enum Specifier value according to the token type
        /// </summary>
        public static Shell.Types.Specifier GetSpecifier(Antlr4.Runtime.IToken token)
        {
            if (token.Type == ShellLexer.TYPE_ARRAY)
                return Shell.Types.Specifier.Array;
            else if (token.Type == ShellLexer.TYPE_BOOL)
                return Shell.Types.Specifier.Bool;
            else if (token.Type == ShellLexer.TYPE_NUMBER)
                return Shell.Types.Specifier.Number;
            else if (token.Type == ShellLexer.TYPE_OBJECT)
                return Shell.Types.Specifier.Object;
            else if (token.Type == ShellLexer.TYPE_STRING)
                return Shell.Types.Specifier.String;
            else
                return Shell.Types.Specifier.Any;
        }

        /// <summary>
        /// Evaluates an expression and returns a Shell.Types.Boolean
        /// </summary>
        public static Shell.Types.Boolean EvaluateExpr(Visitor parser, ShellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            expressionResult = Utility.GetReturnableValue(expressionResult);
            if (!(expressionResult is Shell.Types.Boolean))
                throw new Shell.Problems.UnexpectedType(
                    new Source(context.Start, context.Stop),
                    expressionResult, typeof(Shell.Types.Boolean)
                );

            return (Shell.Types.Boolean) expressionResult;
        }

        /// <summary>
        /// If the IShellReturnable is a Property, unboxes it
        /// </summary>
        public static Shell.Types.IShellReturnable GetReturnableValue(Shell.Types.IShellReturnable returnable)
        {
            if (returnable is Shell.Types.Property prop)
                return prop.Value;

            return returnable;
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

            for (int i = 0; i < parent.ChildCount; i++)
                if (parent.GetChild(i) == context)
                    return i;

            return -1;
        }
    }
}