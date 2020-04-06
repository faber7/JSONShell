using Generated;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Serilog;

namespace Skell.Interpreter
{
    internal class SkellInterpreter
    {
        private static ILogger logger;
        private ITokenSource lexer;
        private SkellParser parser;
        private readonly SkellVisitor visitor;

        public SkellInterpreter()
        {
            logger = Log.ForContext<SkellInterpreter>();
            visitor = new SkellVisitor();
        }

        public void Interprete(string src)
        {
            logger.Verbose($"Source:\n{src.Trim()}");
            ICharStream charStream = CharStreams.fromstring(src);
            lexer = new SkellLexer(charStream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            parser = new SkellParser(tokenStream) {
                BuildParseTree = true
            };
            IParseTree tree = parser.program(); // Since program is our start rule
            logger.Debug($"Parse tree:\n{tree.ToStringTree(parser)}");

            visitor.Visit(tree);
        }
    }
}