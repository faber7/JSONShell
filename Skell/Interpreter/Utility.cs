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
            /// Sets up, executes, and then cleans up function calls.
            /// </summary>
            /// <remark>
            /// Set name to "" if the function is namespaced.
            /// </remark>
            public static Skell.Types.ISkellReturnable ExecuteFunction(
                SkellBaseVisitor<Skell.Types.ISkellReturnable> visitor,
                State state,
                Skell.Types.Function function,
                List<Tuple<int, Skell.Types.ISkellType>> unnamedArgs
            )
            {
                Skell.Types.ISkellReturnable returnValue;
                if (unnamedArgs == null)
                    unnamedArgs = new List<Tuple<int, Types.ISkellType>>();

                var lambda = function.SelectLambda(unnamedArgs);
                if (lambda == null)
                    throw new Skell.Problems.InvalidFunctionCall(function, unnamedArgs);
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

                // set up state with arguments in context
                foreach (var arg in args)
                    if (arg.Item3 is Skell.Types.Property prop)
                        if (lambda is Skell.Types.BuiltinLambda)
                            state.Variables.Set(arg.Item2, prop);
                        else
                            state.Variables.Set(arg.Item2, prop.Value);
                    else 
                        state.Variables.Set(arg.Item2, arg.Item3);

                // allow the function to call itself through its own identifier
                state.Functions.Set(function.name, function);
                
                if (lambda is Skell.Types.UserDefinedLambda udLambda)
                    returnValue = udLambda.Execute(visitor);
                else
                    returnValue = ((Skell.Types.BuiltinLambda) lambda).Execute(state, args);

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
        public static Skell.Types.Namespace LoadNamespace(string path, string rename, Visitor visitor)
        {
            var pathFull = Path.GetFullPath(path);
            var pathDir = Path.GetDirectoryName(pathFull);
            // Read and prepare file
            var src = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            if (src.Last() != '\n')
                src += '\n';

            var charStream = CharStreams.fromstring(src);
            var lexer = new SkellLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new SkellParser(tokenStream);
            var tree = parser.@namespace();

            return new Skell.Types.Namespace(rename, pathDir, tree, visitor);
        }

        /// <summary>
        /// Safely set the value of an indexed variable with an index
        /// </summary>
        /// <remark>
        /// source should refer to the index' source
        /// </remark>
        public static void SetIndex(
            Source source,
            Skell.Types.ISkellIndexable indexable,
            Skell.Types.ISkellType index,
            Skell.Types.ISkellType value
        )
        {
            if (indexable is Skell.Types.Array)
                if (index is Skell.Types.Number num && num.isInt)
                    indexable.Replace(index, value);
                else
                    throw new Skell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Skell.Types.Number)
                    );
            else
                if (index is Skell.Types.String)
                    indexable.Replace(index, value);
                else
                    throw new Skell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Skell.Types.String)
                    );
        }

        /// <summary>
        /// Safely index into an indexable object, checking types
        /// Source should be source of index
        /// </summary>
        public static Skell.Types.ISkellType GetIndex(
            Source source,
            Skell.Types.ISkellIndexable indexable,
            Skell.Types.ISkellType index
        )
        {
            if (indexable is Skell.Types.Array)
                if (!(index is Skell.Types.Number num && num.isInt))
                    throw new Skell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Skell.Types.Number)
                    );
            else if (indexable is Skell.Types.Object)
                if (!(index is Skell.Types.String))
                    throw new Skell.Problems.UnexpectedType(
                        source,
                        index,
                        typeof(Skell.Types.String)
                    );

            if (indexable.ListIndices().Contains(index))
                return indexable.GetMember(index);
            else
                throw new Skell.Problems.IndexOutOfRange(source, index, indexable);
        }

        /// <summary>
        /// Returns the value pointed to by the name, without calling it
        /// </summary>
        public static Skell.Types.ISkellNamedType GetIdentifer(
            Source src, string name,
            State state
        )
        {
            if (state.Names.Available(name))
                throw new Skell.Problems.UndefinedIdentifer(
                    src,
                    name
                );
                    
            return state.Names.DefinitionOf(name);
        }

        /// <summary>
        /// Wrapper over GetIdentifier, calling the returned value with no args if it is a function
        /// </summary>
        public static Skell.Types.ISkellType GetIdentifer(
            Source src, string name,
            Visitor visitor, State state
        )
        {
            var named = Utility.GetIdentifer(src, name, state);
            if (named is Skell.Types.Function fn) {
                var result = Utility.Function.ExecuteFunction(
                        visitor,
                        state,
                        fn,
                        null
                    );
                return Utility.GetSkellType(src, result);
            } else 
                throw new Skell.Problems.UnexpectedType(
                    src, named,
                    typeof(Skell.Types.ISkellType),
                    typeof(Skell.Types.Function)
                );
            
        }

        /// <summary>
        /// Small wrapper over Utility.GetNamespacedIdentifier to call the return value if it is a function
        /// </summary>
        public static Skell.Types.ISkellReturnable GetNamespacedIdentifier(
            SkellParser.NamespacedIdentifierContext context,
            Visitor visitor,
            State state
        )
        {
            var named = Utility.GetNamespacedIdentifier(context, state);

            if (named is Skell.Types.Function fn)
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
        public static Skell.Types.ISkellType GetSkellType(Source src, Skell.Types.ISkellReturnable returnable)
        {
            if (returnable is Skell.Types.ISkellType rdata)
                return rdata;
            else
                throw new Skell.Problems.UnexpectedType(
                    src,
                    returnable,
                    typeof(Skell.Types.ISkellType)
                );
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

            if (state.Names.Available(rootName))
                throw new Skell.Problems.UndefinedIdentifer(
                    new Source(context.IDENTIFIER().First().Symbol),
                    rootName,
                    typeof(Skell.Types.Namespace)
                );

            var nst = state.Names.DefinitionOf(rootName);
            if (!(nst is Skell.Types.Namespace))
                throw new Skell.Problems.UnexpectedType(
                    new Source(context.IDENTIFIER().First().Symbol),
                    nst,
                    typeof(Skell.Types.Namespace)
                );

            var ns = (Skell.Types.Namespace) nst;
            var nsRoot = ns;
            foreach (var id in context.IDENTIFIER().Skip(1).SkipLast(1)) {
                nst = ns.Get(id.GetText());
                if (!(nst is Skell.Types.Namespace))
                    throw new Skell.Problems.UnexpectedType(
                        new Source(context.IDENTIFIER().First().Symbol),
                        nst,
                        typeof(Skell.Types.Namespace)
                    );

                ns = (Skell.Types.Namespace) nst;
            }
            
            var name = context.IDENTIFIER().Last().GetText();
            if (ns.Exists(name)) {
                var result = ns.Get(context.IDENTIFIER().Last().GetText());
                return result;
            } else
                throw new Skell.Problems.InvalidNamespacedIdentifier(
                    new Source(context.IDENTIFIER().First().Symbol, context.IDENTIFIER().Last().Symbol),
                    new Skell.Types.Array(nsRoot.ListNames())
                );
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

                    if (value is Skell.Types.ISkellType)
                        return value.ToString();
                    else
                        return "";
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
            if (context.usableTypeSpecifier() != null)
                return Utility.GetTokenOfUsableTypeSpecifier(context.usableTypeSpecifier());
            
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
        /// Returns true if the data matches the given type specifier
        /// </summary>
        public static bool MatchType(Skell.Types.ISkellInternal data, Skell.Types.Specifier type)
        {
            if (data is Skell.Types.Array && type == Skell.Types.Specifier.Array)
                return true;
            else if (data is Skell.Types.Boolean && type == Skell.Types.Specifier.Bool)
                return true;
            else if (data is Skell.Types.Number && type == Skell.Types.Specifier.Number)
                return true;
            else if (data is Skell.Types.Object && type == Skell.Types.Specifier.Object)
                return true;
            else if (data is Skell.Types.String && type == Skell.Types.Specifier.String)
                return true;
            else if (data is Skell.Types.ISkellType && type == Skell.Types.Specifier.Any)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the enum Specifier value according to the token type
        /// </summary>
        public static Skell.Types.Specifier GetSpecifier(Antlr4.Runtime.IToken token)
        {
            if (token.Type == SkellLexer.TYPE_ARRAY)
                return Skell.Types.Specifier.Array;
            else if (token.Type == SkellLexer.TYPE_BOOL)
                return Skell.Types.Specifier.Bool;
            else if (token.Type == SkellLexer.TYPE_NUMBER)
                return Skell.Types.Specifier.Number;
            else if (token.Type == SkellLexer.TYPE_OBJECT)
                return Skell.Types.Specifier.Object;
            else if (token.Type == SkellLexer.TYPE_STRING)
                return Skell.Types.Specifier.String;
            else
                return Skell.Types.Specifier.Any;
        }

        /// <summary>
        /// Evaluates an expression and returns a Skell.Types.Boolean
        /// </summary>
        public static Skell.Types.Boolean EvaluateExpr(Visitor parser, SkellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            expressionResult = Utility.GetReturnableValue(expressionResult);
            if (!(expressionResult is Skell.Types.Boolean))
                throw new Skell.Problems.UnexpectedType(
                    new Source(context.Start, context.Stop),
                    expressionResult, typeof(Skell.Types.Boolean)
                );

            return (Skell.Types.Boolean) expressionResult;
        }

        /// <summary>
        /// If the ISkellReturnable is a Property, unboxes it
        /// </summary>
        public static Skell.Types.ISkellReturnable GetReturnableValue(Skell.Types.ISkellReturnable returnable)
        {
            if (returnable is Skell.Types.Property prop)
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