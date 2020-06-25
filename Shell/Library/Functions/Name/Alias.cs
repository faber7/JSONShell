using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Shell.Generated;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Name
{
    public class Alias : Shell.Types.BuiltinLambda
    {
        public Alias()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("name", Shell.Types.Specifier.String),
                new Tuple<string, Shell.Types.Specifier>("namespacedid", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var name = (Shell.Types.String) args.First().Item3;
            var namespacedid = (Shell.Types.String) args.Skip(1).First().Item3;

            // test the name to ensure its valid
            var match = Regex.Match(name.contents, @"(\w|_)(\w|\d|_)*");
            if (!match.Success && match.Length != name.contents.Length) {
                Console.WriteLine("Aliasing failed! The given name cannot be set as an identifier.");
                return new Shell.Types.None();
            }

            if (state.Names.Exists(name.contents)) {
                Console.WriteLine("Aliasing failed! The given name is already defined.");
                return new Shell.Types.None();
            }

            // parse the namespaced id
            var charStream = CharStreams.fromstring(namespacedid.contents);
            var lexer = new ShellLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ShellParser(tokenStream) {
                ErrorHandler = new BailErrorStrategy()
            };

            ShellParser.Identifier_namespacedContext context;

            try {
                context = parser.identifier_namespaced();
            } catch (Exception e) {
                Console.WriteLine($"Aliasing failed! Could not parse the given namespaced identifier: {e.Message}");
                return new Shell.Types.None();
            }
            
            var result = Utility.GetNamespacedIdentifier(context, state);
            if (!(result is Shell.Types.Function)) {
                Console.WriteLine("Aliasing failed! The given namespaced id does not refer to a function.");
                return new Shell.Types.None();
            }
            
            // set the alias in the calling context
            var tup = state.contexts.SkipLast(1).Last();

            tup.Item2.Set(name.contents, (Shell.Types.Function) result);

            return new Shell.Types.None();
        }
    }
}