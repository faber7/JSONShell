using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Skell.Generated;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Name
{
    public class Alias : Skell.Types.BuiltinLambda
    {
        public Alias()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("name", Skell.Types.Specifier.String));
            argList.Add(new Tuple<string, Skell.Types.Specifier>("namespacedid", Skell.Types.Specifier.String));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var name = (Skell.Types.String) args.First().Item3;
            var namespacedid = (Skell.Types.String) args.Skip(1).First().Item3;

            // test the name to ensure its valid
            var match = Regex.Match(name.contents, @"(\w|_)(\w|\d|_)*");
            if (!match.Success && match.Length != name.contents.Length) {
                Console.WriteLine("Aliasing failed! The given name cannot be set as an identifier.");
                return new Skell.Types.None();
            }

            if (state.Names.Exists(name.contents)) {
                Console.WriteLine("Aliasing failed! The given name is already defined.");
                return new Skell.Types.None();
            }

            // parse the namespaced id
            var charStream = CharStreams.fromstring(namespacedid.contents);
            var lexer = new SkellLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new SkellParser(tokenStream) {
                ErrorHandler = new BailErrorStrategy()
            };

            SkellParser.NamespacedIdentifierContext context;

            try {
                context = parser.namespacedIdentifier();
            } catch (Exception e) {
                Console.WriteLine($"Aliasing failed! Could not parse the given namespaced identifier: {e.Message}");
                return new Skell.Types.None();
            }
            
            var result = Utility.GetNamespacedIdentifier(context, state);
            if (!(result is Skell.Types.Function fn)) {
                Console.WriteLine("Aliasing failed! The given namespaced id does not refer to a function.");
                return new Skell.Types.None();
            }
            
            // set the alias in the calling context
            var tup = state.contexts.SkipLast(1).Last();

            tup.Item2.Set(name.contents, (Skell.Types.Function) result);

            return new Skell.Types.None();
        }
    }
}