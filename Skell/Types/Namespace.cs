using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;

namespace Skell.Types
{
    class Namespace : Skell.Types.ISkellNamedType
    {
        public Namespace parent;
        public string definitionDirectory;
        public string name;
        private Dictionary<string, Skell.Types.ISkellNamedType> contents;

        /// <summary>
        /// This overloaded constructor is only for internal use (loading builtins)
        /// </summary>
        public Namespace(string nm)
        {
            definitionDirectory = ".";
            contents = new Dictionary<string, Types.ISkellNamedType>();
            this.name = nm;
        }

        /// <summary>
        /// namespace : KW_NAMESPACE IDENTIFIER LCURL EOL? namespaceStmt* RCURL ;
        /// namespaceStmt : EOL | namespaceDecl EOL | namespace EOL | namespaceLoad EOL ;
        /// namespaceDecl : IDENTIFIER OP_ASSGN (expression | function) ;
        /// namespaceLoad : KW_USING STRING (KW_AS IDENTIFIER)? ;
        /// </summary>
        public Namespace(string from, Skell.Generated.SkellParser.NamespaceContext context, Visitor visitor)
        {
            definitionDirectory = from;
            contents = new Dictionary<string, Types.ISkellNamedType>();
            name = context.IDENTIFIER().GetText();
            foreach (var stmt in context.namespaceStmt()) {
                var nsContext = stmt.@namespace();
                var nsDecl = stmt.namespaceDecl();
                var nsLoad = stmt.namespaceLoad();

                if (nsContext != null) {
                    var ns = new Namespace(definitionDirectory, nsContext, visitor);
                    ns.parent = this;
                    Set(ns.name, ns);
                } else if (nsDecl != null) {
                    string nm = nsDecl.IDENTIFIER().GetText();
                    if (nsDecl.expression() != null) {
                        var value = visitor.VisitExpression(nsDecl.expression());
                        if (value is Skell.Types.ISkellType val) {
                            Set(nm, val);
                        } else {
                            throw new Skell.Problems.UnexpectedType(
                                new Source(nsDecl.expression().Start, nsDecl.expression().Stop),
                                value, typeof(Skell.Types.ISkellType)
                            );
                        }
                    } else if (nsDecl.function() != null) {
                        if (Exists(nm)) {
                            var val = Get(nm);
                            if (val is Skell.Types.Function fn) {
                                fn.AddUserDefinedLambda(nsDecl.function());
                            } else {
                                throw new Skell.Problems.InvalidDefinition(
                                    new Source(nsDecl.function().Start, nsDecl.function().Stop),
                                    nm
                                );
                            }
                        } else {
                            var fn = new Skell.Types.Function(nm);
                            fn.AddUserDefinedLambda(nsDecl.function());
                            Set(nm, fn);
                        }
                    }
                } else if (nsLoad != null) {
                    var name = Utility.GetString(nsLoad.STRING().GetText(), visitor);
                    string newPath;
                    if (definitionDirectory == "." && name.contents.StartsWith("./")) {
                        newPath = name.contents;
                    } else if (definitionDirectory != "." && name.contents.StartsWith("/")) {
                        newPath = name.contents;
                    } else {
                        if (!(definitionDirectory.EndsWith('/')) && name.contents.StartsWith("./")) {
                            newPath = definitionDirectory + name.contents.Substring(1);
                        } else {
                            newPath = definitionDirectory + name.contents;
                        }
                    }
                    var ns = Utility.LoadNamespace(
                        name.contents,
                        nsLoad.IDENTIFIER() != null ? nsLoad.IDENTIFIER().GetText() : "",
                        visitor
                    );
                    ns.parent = this;
                    Set(ns.name, ns);
                }
            }
        }

        public string GetFullName()
        {
            if (parent == null)
                return name;
            else
                return $"{parent.GetFullName()}.{name}";
        }

        public Skell.Types.String[] ListNames()
        {
            if (contents.Keys.Count == 0)
                return new Skell.Types.String[0];

            var list = new List<Skell.Types.String>();
            foreach (var pair in contents) {
                if (pair.Value is Skell.Types.Namespace ns)
                    list.AddRange(ns.ListNames());

                list.Add(new Skell.Types.String($"{GetFullName()}.{pair.Key}"));
            }

            return list.ToArray();
        }

        public bool Exists(string name) => contents.ContainsKey(name);

        public void Set(string name, Skell.Types.ISkellNamedType value)
        {
            contents[name] = value;
        }

        public Skell.Types.ISkellNamedType Get(string name) => contents[name];
    }
}