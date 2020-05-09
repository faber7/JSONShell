using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Skell.Generated;

namespace Skell.Types
{
    /// <summary>
    /// Represents executable functions(both user-defined and builtin) that can be overloaded
    /// </summary>
    public class Function : ISkellNamedType
    {
        public string name;
        public List<Lambda> definitions = new List<Lambda>();

        public Function(string n)
        {
            name = n;
        }

        public void AddUserDefinedLambda(
            SkellParser.FunctionContext ctx
        )
        {
            var lambda = new UserDefinedLambda(ctx);
            foreach(var definition in definitions)
                if (lambda.argList.Count == definition.argList.Count) {
                    var tokens = lambda.argList.Zip(definition.argList,
                        (t1, t2) => new { LambdaToken = t1, DefinitionToken = t2 }
                    );
                    int i = 0;
                    foreach (var tokenPair in tokens) {
                        var lt = tokenPair.LambdaToken;
                        var dt = tokenPair.DefinitionToken;
                        string any = SkellParser.DefaultVocabulary.GetDisplayName(SkellLexer.TYPE_ANY);
                        any = any.Substring(1, any.Length - 2);
                        if (lt.Item2.Text == any || dt.Item2.Text == any || lt.Item2.Text == dt.Item2.Text)
                            i++;
                    }
                    if (i == lambda.argList.Count)
                        throw new Skell.Problems.InvalidFunctionDefinition(
                            new Interpreter.Source(ctx.Start, ctx.Stop),
                            this, lambda
                        );
                }

            definitions.Add(lambda);
        }

        public void AddBuiltinLambda(
            List<Tuple<string, IToken>> args,
            Func<ISkellReturnable> fn
        )
        {
            var lambda = new BuiltinLambda(args, fn);
            foreach(var definition in definitions) {
                if (lambda.argList.Count == definition.argList.Count 
                    && !lambda.argList.Except(definition.argList).Any()
                )
                    throw new Skell.Problems.InvalidFunctionDefinition(this, lambda);
            }
            definitions.Add(lambda);
        }

        public Lambda SelectLambda(
            List<Tuple<int, ISkellType>> unnamedArgs
        )
        {
            foreach (var lambda in definitions.Where(lambda => lambda.Arity() == unnamedArgs.Count)) { 
                var args = lambda.NameArguments(unnamedArgs);
                var extra = lambda.GetExtraArgs(args);
                var invalid = lambda.GetInvalidArgs(args);

                if (extra.Count == 0 && invalid.Count == 0)
                    return lambda;
            }
            return null;
        }
    }
}