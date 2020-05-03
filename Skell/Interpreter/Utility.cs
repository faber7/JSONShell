using System.Collections.Generic;
using System.Linq;
using Skell.Generated;

namespace Skell.Interpreter
{
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
            throw new Skell.Problems.UnexpectedType(data, typeof(Skell.Types.Lambda));
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