using System;
using Generated;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Interpreter
{
    class SkellInterpreter
    {
        ITokenSource lexer;
        SkellParser parser;

        public void interprete(string src)
        {
            ICharStream charStream = CharStreams.fromstring(src);
            lexer = new SkellLexer(charStream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            parser = new SkellParser(tokenStream);
            parser.BuildParseTree = true;
            IParseTree tree = parser.program(); // Since program is our start rule
            Console.WriteLine(tree.ToStringTree(parser));
        }
    }
}