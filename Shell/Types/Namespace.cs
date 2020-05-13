using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;

namespace Shell.Types
{
    public class Namespace : Shell.Types.IShellNamed
    {
        public Namespace parent;
        public string definitionDirectory;
        public string name;
        private readonly Dictionary<string, Shell.Types.IShellNamed> contents;

        /// <summary>
        /// This overloaded constructor is only for internal use (loading builtins)
        /// </summary>
        public Namespace(string nm)
        {
            definitionDirectory = ".";
            contents = new Dictionary<string, Types.IShellNamed>();
            this.name = nm;
        }

        /// <summary>
        /// namespace : KW_NAMESPACE IDENTIFIER LCURL EOL? namespaceStmt* RCURL ;
        /// namespaceStmt : EOL | namespaceDecl EOL | namespace EOL | namespaceLoad EOL ;
        /// namespaceDecl : IDENTIFIER OP_ASSGN (expression | function) ;
        /// namespaceLoad : KW_USING STRING (KW_AS IDENTIFIER)? ;
        /// </summary>
        public Namespace(string name, string from, Shell.Generated.ShellParser.NamespaceContext context, Visitor visitor)
        {
            definitionDirectory = from;
            contents = new Dictionary<string, Types.IShellNamed>();
            this.name = name;
            if (name == "")
                this.name = context.IDENTIFIER().GetText();
            foreach (var stmt in context.namespaceStmt()) {
                var nsContext = stmt.@namespace();
                var nsDecl = stmt.namespaceDecl();
                var nsLoad = stmt.namespaceLoad();

                if (nsContext != null) {
                    var ns = new Namespace("", definitionDirectory, nsContext, visitor)
                    {
                        parent = this
                    };
                    Set(ns.name, ns);
                } else if (nsDecl != null) {
                    string nm = nsDecl.IDENTIFIER().GetText();
                    if (nsDecl.expression() != null) {
                        var value = visitor.VisitExpression(nsDecl.expression());
                        if (value is Shell.Types.IShellData val) {
                            Set(nm, val);
                        } else {
                            throw new Shell.Problems.UnexpectedType(
                                new Source(nsDecl.expression().Start, nsDecl.expression().Stop),
                                value, typeof(Shell.Types.IShellData)
                            );
                        }
                    } else if (nsDecl.function() != null) {
                        if (Exists(nm)) {
                            var val = Get(nm);
                            if (val is Shell.Types.Function fn) {
                                fn.AddUserDefinedLambda(nsDecl.function());
                            } else {
                                throw new Shell.Problems.InvalidDefinition(
                                    new Source(nsDecl.function().Start, nsDecl.function().Stop),
                                    nm
                                );
                            }
                        } else {
                            var fn = new Shell.Types.Function(nm);
                            fn.AddUserDefinedLambda(nsDecl.function());
                            Set(nm, fn);
                        }
                    }
                } else if (nsLoad != null) {
                    name = Utility.GetString(nsLoad.STRING().GetText(), visitor).contents;
                    string newPath;
                    if (definitionDirectory == "." && name.StartsWith("./")) {
                        newPath = name;
                    } else if (definitionDirectory != "." && name.StartsWith("/")) {
                        newPath = name;
                    } else {
                        if (!(definitionDirectory.EndsWith('/')) && name.StartsWith("./")) {
                            newPath = definitionDirectory + name.Substring(1);
                        } else {
                            newPath = definitionDirectory + name;
                        }
                    }
                    var ns = Utility.LoadNamespace(
                        name,
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

        public Shell.Types.String[] ListNames()
        {
            if (contents.Keys.Count == 0)
                return new Shell.Types.String[0];

            var list = new List<Shell.Types.String>();
            foreach (var pair in contents) {
                if (pair.Value is Shell.Types.Namespace ns)
                    list.AddRange(ns.ListNames());

                list.Add(new Shell.Types.String($"{GetFullName()}.{pair.Key}"));
            }

            return list.ToArray();
        }

        public bool Exists(string name) => contents.ContainsKey(name);

        public void Set(string name, Shell.Types.IShellNamed value)
        {
            contents[name] = value;
        }

        public Shell.Types.IShellNamed Get(string name) => contents[name];
    }
}