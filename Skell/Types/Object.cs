using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Skell.Types
{
    public class Object : ISkellIndexable
    {
        private readonly Dictionary<String, ISkellType> dict;

        public Object()
        {
            dict = new Dictionary<String, ISkellType>();
        }

        /// <summary>
        /// Creates an object with the given keys and values
        /// If the number of keys and values aren't equal, throws IndexOutOfRangeException()
        /// </summary>
        public Object(String[] k, ISkellType[] v)
        {
            if (k.Length != v.Length)
                throw new System.IndexOutOfRangeException();

            dict = new Dictionary<String, ISkellType>();
            for (int i = 0; i < k.Length; i++)
                dict.Add(k[i], v[i]);
        }

        public Number Count() => new Number(dict.Count);

        public ISkellType[] ListIndices() => dict.Keys.ToArray();
        public bool Exists(ISkellType index)
        {
            return (index is String s && dict.ContainsKey(s));
        }

        public ISkellType GetMember(ISkellType index) => dict[(Skell.Types.String) index];

        public void Replace(ISkellType index, ISkellType value) => dict[(Skell.Types.String) index] = value;

        public void Insert(ISkellType index, ISkellType value) => dict[(Skell.Types.String) index] = value;

        public void Delete(ISkellType index) => dict.Remove((Skell.Types.String) index);
        public ISkellReturnable IndexOf(ISkellType value)
        {
            foreach (var pair in dict)
                if (pair.Value == value)
                    return pair.Key;
            
            return new Skell.Types.None();
        }

        public static Number operator +(Object a) => throw new Skell.Problems.InvalidOperation("+", a);
        public static Number operator -(Object a) => throw new Skell.Problems.InvalidOperation("-", a);
        public static Number operator +(Object a, Object b) => throw new Skell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(Object a, Object b) => throw new Skell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(Object a, Object b) => throw new Skell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(Object a, Object b) => throw new Skell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(Object a) => throw new Skell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(Object a1, Object a2) => new Boolean(a1.Equals(a2));
        public static Boolean operator !=(Object a1, Object a2) => new Boolean(!a1.Equals(a2));
        public static Boolean operator >(Object a, Object b) => throw new Skell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(Object a, Object b) => throw new Skell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(Object a, Object b) => throw new Skell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(Object a, Object b) => throw new Skell.Problems.InvalidOperation("<=", a, b);

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