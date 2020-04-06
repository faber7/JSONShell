using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Skell.Data
{
    public class Object : ISkellIndexableData
    {
        private readonly Dictionary<String, ISkellData> dict;

        public Object()
        {
            dict = new Dictionary<String, ISkellData>();
        }

        public Object(String[] k, ISkellData[] v)
        {
            if (k.Length != v.Length) {
                throw new System.NotImplementedException();
            }
            dict = new Dictionary<String, ISkellData>();
            for (int i = 0; i < k.Length; i++) {
                dict.Add(k[i], v[i]);
            }
        }

        public int Count() => dict.Count;

        public ISkellData GetMember(ISkellData index)
        {
            if (index is String s && dict.ContainsKey(s)) {
                return dict[s];
            }
            throw new System.NotImplementedException();
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