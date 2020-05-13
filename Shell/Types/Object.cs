using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Shell.Types
{
    public class Object : IShellIndexable
    {
        private readonly Dictionary<String, IShellData> dict;

        public Object()
        {
            dict = new Dictionary<String, IShellData>();
        }

        /// <summary>
        /// Creates an object with the given keys and values
        /// If the number of keys and values aren't equal, throws IndexOutOfRangeException()
        /// </summary>
        public Object(String[] k, IShellData[] v)
        {
            if (k.Length != v.Length)
                throw new System.IndexOutOfRangeException();

            dict = new Dictionary<String, IShellData>();
            for (int i = 0; i < k.Length; i++)
                dict.Add(k[i], v[i]);
        }

        public Number Count() => new Number(dict.Count);

        public IShellData[] ListIndices() => dict.Keys.ToArray();
        public IShellData[] ListValues() => dict.Values.ToArray();

        public bool Exists(IShellData index)
        {
            return (index is String s && dict.ContainsKey(s));
        }

        public bool IsHomogeneous(Specifier type)
        {
            foreach (var pair in dict)
                if (!Shell.Interpreter.Utility.MatchType(pair.Value, type))
                    return false;
            
            return true;
        }

        public IShellData GetMember(IShellData index) => dict[(Shell.Types.String) index];

        public void Replace(IShellData index, IShellData value) => dict[(Shell.Types.String) index] = value;

        public void Insert(IShellData index, IShellData value) => dict[(Shell.Types.String) index] = value;

        public void Delete(IShellData index) => dict.Remove((Shell.Types.String) index);
        public IShellReturnable IndexOf(IShellData value)
        {
            foreach (var pair in dict)
                if (pair.Value == value)
                    return pair.Key;
            
            return new Shell.Types.None();
        }

        public static Number operator +(Object a) => throw new Shell.Problems.InvalidOperation("+", a);
        public static Number operator -(Object a) => throw new Shell.Problems.InvalidOperation("-", a);
        public static Number operator +(Object a, Object b) => throw new Shell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(Object a, Object b) => throw new Shell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(Object a, Object b) => throw new Shell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(Object a, Object b) => throw new Shell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(Object a) => throw new Shell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(Object a1, Object a2) => new Boolean(a1.Equals(a2));
        public static Boolean operator !=(Object a1, Object a2) => new Boolean(!a1.Equals(a2));
        public static Boolean operator >(Object a, Object b) => throw new Shell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(Object a, Object b) => throw new Shell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(Object a, Object b) => throw new Shell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(Object a, Object b) => throw new Shell.Problems.InvalidOperation("<=", a, b);

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("{ ");
            for (int i = 0; i < dict.Count; i++) {
                var item = dict.ElementAt(i);
                var key = item.Key;
                var value = item.Value;
                s.Append(key).Append(" : ").Append(value);
                if (i != dict.Count - 1)
                    s.Append(", ");
            }
            s.Append(" }");
            return s.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj is Object ob)
                return dict.Count == ob.dict.Count && !dict.Except(ob.dict).Any();

            return false;
        }
        public override int GetHashCode() => dict.GetHashCode();
    }
}