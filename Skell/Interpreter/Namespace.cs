using System.Collections.Generic;
using System.Linq;

namespace Skell.Interpreter
{
    class Namespace : Skell.Types.ISkellNamedType
    {
        public string name;
        private Dictionary<string, Skell.Types.ISkellNamedType> contents;

        /// <summary>
        /// namespace : KW_NAMESPACE IDENTIFIER LCURL EOL? namespaceStmt* RCURL ;
        /// namespaceStmt : EOL | namespaceDecl EOL | namespace EOL ;
        /// namespaceDecl : IDENTIFIER OP_ASSGN (expression | function) ;
        /// </summary>
        public Namespace(Skell.Generated.SkellParser.NamespaceContext context, Visitor visitor)
        {
            contents = new Dictionary<string, Types.ISkellNamedType>();
            name = context.IDENTIFIER().GetText();
            foreach (var stmt in context.namespaceStmt()) {
                var nsContext = stmt.@namespace();
                var nsDecl = stmt.namespaceDecl();

                if (nsContext != null) {
                    var ns = new Namespace(nsContext, visitor);
                    Set(ns.name, ns);
                } else if (nsDecl != null) {
                    string nm = nsDecl.IDENTIFIER().GetText();
                    if (nsDecl.expression() != null) {
                        var value = visitor.VisitExpression(nsDecl.expression());
                        if (value is Skell.Types.ISkellType val) {
                            Set(nm, val);
                        } else {
                            throw new Skell.Problems.UnexpectedType(value, typeof(Skell.Types.ISkellType));
                        }
                    } else if (nsDecl.function() != null) {
                        if (Exists(nm)) {
                            var val = Get(nm);
                            if (val is Skell.Types.Function fn) {
                                fn.AddUserDefinedLambda(nsDecl.function());
                            } else {
                                throw new Skell.Problems.InvalidDefinition(nm, val);
                            }
                        } else {
                            var fn = new Skell.Types.Function(nm);
                            fn.AddUserDefinedLambda(nsDecl.function());
                            Set(nm, fn);
                        }
                    }
                }
            }
        }

        public Skell.Types.String[] ListNames()
        {
            if (contents.Keys.Count == 0) {
                return new Skell.Types.String[0];
            }
            return contents.Keys.Select((a, b) => new Skell.Types.String(a)).ToArray();
        }

        public bool Exists(string name) => contents.ContainsKey(name);

        public void Set(string name, Skell.Types.ISkellNamedType value)
        {
            contents[name] = value;
        }

        public Skell.Types.ISkellNamedType Get(string name) => contents[name];
        
        public Namespace GetNamespace(string name)
        {
            if (Exists(name) && contents[name] is Namespace ns) {
                return ns;
            } else {
                throw new Skell.Problems.InvalidNamespace(
                    name,
                    contents.Keys.Where((name) => contents[name] is Namespace)
                        .Select((a, b) => new Skell.Types.String(a)).ToArray()
                );
            }
        }
    }
}