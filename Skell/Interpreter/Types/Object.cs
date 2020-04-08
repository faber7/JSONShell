using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Skell.Types
{
    public class Object : ISkellIndexableType
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
            if (k.Length != v.Length) {
                throw new System.IndexOutOfRangeException();
            }
            dict = new Dictionary<String, ISkellType>();
            for (int i = 0; i < k.Length; i++) {
                dict.Add(k[i], v[i]);
            }
        }

        public int Count() => dict.Count;

        public ISkellType GetMember(ISkellType index)
        {
            if (index is String s && dict.ContainsKey(s)) {
                return dict[s];
            }
            throw new Skell.Error.IndexOutOfRange(index, new Skell.Types.Array(dict.Keys.ToArray()));
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("{ ");
            for (int i = 0; i < dict.Count; i++) {
                var item = dict.ElementAt(i);
                var key = item.Key;
                var value = item.Value;
                s.Append(key).Append(" : ").Append(value);
                if (i != dict.Count - 1) {
                    s.Append(", ");
                }
            }
            s.Append(" }");
            return s.ToString();
        }
    }
}