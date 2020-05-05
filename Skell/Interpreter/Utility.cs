using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Generated;

namespace Skell.Interpreter
{
    internal static class Utility
    {
        public static class Function
        {
            public static void SetupContextForFunction(
                Skell.Types.Function function,
                Context context,
                List<Tuple<int, string, Skell.Types.ISkellType>> args
            ) {
                context.Set(function.name, function);
                foreach (var arg in args) {
                    context.Set(arg.Item2, arg.Item3);
                }
            }

            /// <summary>
            /// Retrieves the extra arguments that are to be passed to the function.
            /// Use with GetInvalidArgs().
            /// </summary>
            public static List<Tuple<int, Skell.Types.ISkellType>> GetExtraArgs(
                List<Tuple<string, Antlr4.Runtime.IToken>> lambdaArgs,
                List<Tuple<int, string, Skell.Types.ISkellType>> args
            )  
            {
                var extra = new List<Tuple<int, Skell.Types.ISkellType>>();
                for (int i = lambdaArgs.Count; i < args.Count; i++) {
                    extra.Add(new Tuple<int, Types.ISkellType>(i, args[i].Item3));
                }
                return extra;
            }

            /// <summary>
            /// Retrieves the arguments that do not have the same type.
            /// Use with GetExtraArgs().
            /// </summary>
            public static List<Tuple<int, string, Antlr4.Runtime.IToken>> GetInvalidArgs(
                List<Tuple<string, Antlr4.Runtime.IToken>> lambdaArgs,
                List<Tuple<int, string, Skell.Types.ISkellType>> args
            )
            {
                var ret = new List<Tuple<int, string, Antlr4.Runtime.IToken>>();
                int i = 0;
                foreach (var arg in lambdaArgs) {
                    if (i < args.Count && !MatchType(args[i].Item3, arg.Item2)) {
                        ret.Add(new Tuple<int, string, Antlr4.Runtime.IToken>(i, arg.Item1, arg.Item2));
                    }
                    i++;
                }
                return ret;
            }
        }

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
        /// Returns the user-defined function if it exists
        /// </summary>
        public static Skell.Types.Function GetFunction(string name, Context context)
        {
            Skell.Types.ISkellType data = context.Get(name);
            if (data is Skell.Types.Function fn) {
                return fn;
            }
            throw new Skell.Problems.UnexpectedType(data, typeof(Skell.Types.Function));
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
        /// Evaluates an expression and returns a Skell.Types.Boolean
        /// </summary>
        public static Skell.Types.Boolean EvaluateExpr(Visitor parser, SkellParser.ExpressionContext context)
        {
            var expressionResult = parser.VisitExpression(context);
            if (!(expressionResult is Skell.Types.Boolean)) {
                throw new Skell.Problems.UnexpectedType(expressionResult, typeof(Skell.Types.Boolean));
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