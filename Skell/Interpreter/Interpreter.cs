using Skell.Generated;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Serilog;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Skell.Interpreter
{
    public class SyntaxErrorListener : BaseErrorListener
    {
        override public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e
        )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Syntax error at line {line}:{charPositionInLine}\n");
            var expected = e.GetExpectedTokens()
                    .ToIntegerList()
                    .Select((tok, index) => {
                        var str = recognizer.Vocabulary.GetLiteralName(tok);
                        return (str != null) ? str : recognizer.Vocabulary.GetDisplayName(tok);
                    }).ToList();
            sb.Append("Expected one of: " +  String.Join(", ", expected));
            throw new ParseFailure(sb.ToString());
        }
    }

    public class ParseFailure : System.Exception
    {
        new public string Message;
        public ParseFailure(string msg)
        {
            Message = msg;
        }
    }

    internal class Interpreter
    {
        private static ILogger logger;
        private ITokenSource lexer;
        private SkellParser parser;
        private readonly Visitor visitor;

        public Interpreter()
        {
            logger = Log.ForContext<Interpreter>();
            visitor = new Visitor();
        }

        public int Interprete(string src)
        {
            try {
                logger.Verbose($"Source:\n{src.Trim()}");
                ICharStream charStream = CharStreams.fromstring(src);
                lexer = new SkellLexer(charStream);
                ITokenStream tokenStream = new CommonTokenStream(lexer);
                parser = new SkellParser(tokenStream) {
                    BuildParseTree = true,
                    ErrorHandler = new BailErrorStrategy()
                };
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new SyntaxErrorListener());
                IParseTree tree = parser.program(); // Since program is our start rule
                logger.Debug($"Parse tree:\n{tree.ToStringTree(parser)}");

                visitor.tokenSource = lexer.InputStream;
                visitor.Visit(tree);
            } catch (ParseFailure e) {
                logger.Fatal(e.Message);
                return 1;
            }
            return 0;
        }

        public int RunFile(string path)
        {
            byte[] input = File.ReadAllBytes(path);
            string inputStr = Encoding.ASCII.GetString(input);
            if (inputStr.Last() != '\n') {
                inputStr += '\n';
            }
            return Interprete(inputStr);
        }

        public int RunPrompt()
        {
            Console.WriteLine("Press Ctrl+D to exit the prompt.");

            Stack<char> stack = new Stack<char>();
            bool continueInput = false;
            char top;
            string input = "", inp;
            Console.Write("> ");
            while ((inp = Console.In.ReadLine()) != null) {
                continueInput = false;
                // Atleast verify whether the brackets are balanced
                foreach (char c in inp) {
                    if (c == '(')
                        stack.Push(')');
                    else if (c == '{')
                        stack.Push('}');
                    else if (c == '[')
                        stack.Push(']');
                    else if (stack.TryPeek(out top) && c == top) 
                        stack.Pop();
                    else
                        continueInput = true;
                }

                continueInput = (stack.Count > 0) ? true : false ;

                input = input + inp;
                if (!continueInput) {
                    Interprete(input + '\n');
                    Console.Write("\n> ");
                }
            }
            return 0;
        }
    }
}