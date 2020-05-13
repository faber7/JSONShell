using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Shell.Generated;

namespace Shell.Types
{
    /// <summary>
    /// Represents executable functions(both user-defined and builtin) that can be overloaded
    /// </summary>
    public class Function : IShellNamed
    {
        public string name;
        public List<Lambda> definitions = new List<Lambda>();

        public Function(string n)
        {
            name = n;
        }

        public Shell.Types.String[] GetStringDefinitions()
        {
            return definitions.Select((lambda) => new Shell.Types.String(lambda.ToString())).ToArray();
        }

        public void AddUserDefinedLambda(
            ShellParser.FunctionContext ctx
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
                        if (lt.Item2 == Specifier.Any || dt.Item2 == Specifier.Any || lt.Item2 == dt.Item2)
                            i++;
                    }
                    if (i == lambda.argList.Count)
                        throw new Shell.Problems.InvalidFunctionDefinition(
                            new Interpreter.Source(ctx.Start, ctx.Stop),
                            this, lambda
                        );
                }

            definitions.Add(lambda);
        }

        public void AddBuiltinLambda(BuiltinLambda lambda)
        {
            foreach(var definition in definitions) {
                if (lambda.argList.Count == definition.argList.Count 
                    && !lambda.argList.Except(definition.argList).Any()
                )
                    throw new Shell.Problems.InvalidFunctionDefinition(this, lambda);
            }
            definitions.Add(lambda);
        }

        public Lambda SelectLambda(
            List<Tuple<int, IShellData>> unnamedArgs
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