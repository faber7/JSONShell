using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Serilog;
using Skell.Generated;

namespace Skell.Interpreter
{
    internal static class Utility
    {
        public static class Function
        {
            private static readonly ILogger logger = Log.ForContext(typeof(Function));

            /// <summary>
            /// Retrieves the function, creating a new one if necessary
            /// </summary>
            public static Skell.Types.Function Get(State state, string name)
            {
                Skell.Types.Function fn;
                if (state.Names.Exists(name) && state.Names.DefinedAs(name, typeof(Skell.Types.Function))) {
                    fn = state.Functions.Get(name);
                    return fn;
                }
                fn = new Skell.Types.Function(name);
                state.Functions.Set(name, fn);
                return fn;
            }

            /// <summary>
            /// Sets up, executes, and then cleans up function calls
            /// </summary>
            public static Skell.Types.ISkellReturnable ExecuteFunction(
                SkellBaseVisitor<Skell.Types.ISkellReturnable> visitor,
                State state,
                Skell.Types.Function function,
                List<Tuple<int, Skell.Types.ISkellType>> unnamedArgs
            )
            {
                Skell.Types.ISkellReturnable returnValue;
                var lambda = function.SelectLambda(unnamedArgs);
                var args = lambda.NameArguments(unnamedArgs);

                string argString = "";
                if (args.Count > 0) {
                    var sb = new StringBuilder(" ");
                    for (int i = 0; i < args.Count; i++) {
                        sb.Append($"{args[i].Item2}:{args[i].Item3}");
                        if (i + 1 != args.Count) {
                            sb.Append(" ");
                        }
                    }
                    argString = sb.ToString();
                }
                
                // pre-setup for state
                state.ENTER_RETURNABLE_CONTEXT($"{function.name}{argString}");
                // set up state with arguments in context
                foreach (var arg in args) {
                    state.Variables.Set(arg.Item2, arg.Item3);
                }
                
                if (lambda is Skell.Types.UserDefinedLambda udLambda) {
                    returnValue = udLambda.Execute(visitor);
                } else {
                    returnValue = ((Skell.Types.BuiltinLambda) lambda).Execute();
                }

                //cleanup
                if (state.has_returned()) {
                    logger.Debug($"{function.name} returned {returnValue}");
                    state.end_return();
                }
                state.EXIT_RETURNABLE_CONTEXT();

                return returnValue;
            }
        }

        /// <summary>
        /// Loads a namespace definition from file
        /// </summary>
        /// <remark>
        /// Pass rename="" to load namespace with original name
        /// </remark>
        public static Skell.Types.Namespace LoadNamespace(string path, string rename, Visitor visitor)
        {
            var pathFull = Path.GetFullPath(path);
            var pathDir = Path.GetDirectoryName(pathFull);
            // Read and prepare file
            var src = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            if (src.Last() != '\n') {
                src += '\n';
            }

            var charStream = CharStreams.fromstring(src);
            var lexer = new SkellLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new SkellParser(tokenStream);
            var tree = parser.@namespace();

            return new Skell.Types.Namespace(pathDir, tree, visitor);
        }

        /// <summary>
        /// Retrieve the index of an indexing operation
        /// </summary>
        public static Skell.Types.ISkellType GetIndexFromPrimary(SkellParser.PrimaryContext primary, Visitor visitor, State state)
        {
            if (primary.STRING() != null) {
                return Utility.GetString(primary.STRING().GetText(), visitor);
            } else if (primary.NUMBER() != null) {
                return new Skell.Types.Number(primary.NUMBER().GetText());
            } else {
                return state.Variables.Get(primary.IDENTIFIER().GetText());
            }
        }

        /// <summary>
        /// namespacedIdentifier : (IDENTIFIER SYM_PERIOD)+ IDENTIFIER ;
        /// </summary>
        public static Skell.Types.ISkellNamedType GetNamespacedIdentifier(
            SkellParser.NamespacedIdentifierContext context,
            State state
        )
        {
            var rootName = context.IDENTIFIER().First().GetText();

            if (state.Names.Available(rootName)) {
                throw new Skell.Problems.UndefinedIdentifer(
                    new Source(context.IDENTIFIER().First().Symbol),
                    rootName,
                    typeof(Skell.Types.Namespace)
                );
            }

            var nst = state.Names.DefinitionOf(rootName);
            if (!(nst is Skell.Types.Namespace)) {
                throw new Skell.Problems.UnexpectedType(
                    new Source(context.IDENTIFIER().First().Symbol),
                    nst,
                    typeof(Skell.Types.Namespace)
                );
            }
            Skell.Types.Namespace ns = (Skell.Types.Namespace) nst;
            var nsRoot = ns;
            foreach (var id in context.IDENTIFIER().Skip(1).SkipLast(1)) {
                nst = ns.Get(id.GetText());
                if (!(nst is Skell.Types.Namespace)) {
                    throw new Skell.Problems.UnexpectedType(
                        new Source(context.IDENTIFIER().First().Symbol),
                        nst,
                        typeof(Skell.Types.Namespace)
                    );
                }
                ns = (Skell.Types.Namespace) nst;
            }
            
            var name = context.IDENTIFIER().Last().GetText();
            if (ns.Exists(name)) {
                var result = ns.Get(context.IDENTIFIER().Last().GetText());
                return result;
            } else {
                throw new Skell.Problems.InvalidNamespacedIdentifier(
                    new Source(context.IDENTIFIER().First().Symbol, context.IDENTIFIER().Last().Symbol),
                    new Skell.Types.Array(nsRoot.ListNames())
                );
            }
        }

        /// <summary>
        /// Returns a Skell.Types.String for a STRING token's text contents.
        /// Requires visitor to handle string substitution
        /// </summary>
        /// <remark>
        /// STRING : SYM_QUOTE (ESC | SAFECODEPOINT | SYM_DOLLAR LCURL ~[}]* RCURL)* SYM_QUOTE ;
        /// </remark>
        public static Skell.Types.String GetString(
            string contents,
            Visitor visitor
        )
        {
            contents = contents.Remove(0, 1);
            contents = contents.Remove(contents.Length - 1, 1);
            contents = Regex.Replace(
                contents,
                @"\$\{.*\}",
                m => {
                    string src = m.Value.Remove(0, 2);
                    src = src.Remove(src.Length - 1, 1);
                        
                    var charStream = CharStreams.fromstring(src);
                    var lexer = new SkellLexer(charStream);
                    var tokenStream = new CommonTokenStream(lexer);
                    var parser = new SkellParser(tokenStream);
                    var tree = parser.expression();

                    var dupSrc = visitor.tokenSource;
                    visitor.tokenSource = charStream;
                    var value = visitor.VisitExpression(tree);
                    visitor.tokenSource = dupSrc;

                    if (value is Skell.Types.ISkellType) {
                        return value.ToString();
                    } else {
                        return "";
                    }
                }
            );
            return new Skell.Types.String(contents);
        }

        /// <summary>
        /// Returns the token for the given typeSpecifier context
        /// </summary>
        /// <remark>
        /// typeSpecifier : usableTypeSpecifier | TYPE_ANY ;
        /// usableTypeSpecifier : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTokenOfTypeSpecifier(SkellParser.TypeSpecifierContext context)
        {
            if (context.usableTypeSpecifier() != null) {
                return Utility.GetTokenOfUsableTypeSpecifier(context.usableTypeSpecifier());
            }
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }
        
        /// <summary>
        /// Returns the token for the given usableTypeSpecifier context
        /// </summary>
        /// <remark>
        /// typeSpecifier : usableTypeSpecifier | TYPE_ANY ;
        /// usableTypeSpecifier : TYPE_OBJECT | TYPE_ARRAY | TYPE_NUMBER | TYPE_STRING | TYPE_BOOL ;
        /// </remark>
        public static Antlr4.Runtime.IToken GetTokenOfUsableTypeSpecifier(SkellParser.UsableTypeSpecifierContext context)
        {
            return (Antlr4.Runtime.IToken) context.children[0].Payload;
        }

        /// <summary>
        /// Returns true if the data matches the given type specifier token
        /// </summary>
        public static bool MatchType(Skell.Types.ISkellInternal data, Antlr4.Runtime.IToken token)
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
            } else if (data is Skell.Types.ISkellType && token.Type == SkellLexer.TYPE_ANY) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates an expression and returns a Skell.Types.Boolean
        /// </summary>
        public static Skell.Types.Boolean EvaluateExpr(Visitor parser, SkellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            if (!(expressionResult is Skell.Types.Boolean)) {
                throw new Skell.Problems.UnexpectedType(
                    new Source(context.Start, context.Stop),
                    expressionResult, typeof(Skell.Types.Boolean)
                );
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