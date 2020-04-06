using System.Text;
using System.Linq;

namespace Skell.Data
{
    class Object : SkellIndexableData
    {
        private readonly Skell.Data.String[] keys;
        private readonly SkellData[] values;

        public Object(Skell.Data.String[] k, SkellData[] v)
        {
            if (k.Length != v.Length)
            {
                throw new System.NotImplementedException();
            }
            keys = k;
            values = v;
        }

        public int Count() => keys.Length;

        public SkellData GetMember(SkellData index)
        {
            if (index is String s && keys.Contains(s)) {
                int n = keys.Select((str, i) => new {i, str})
                    .Where(t => t.str == s)
                    .Select(t => t.i)
                    .First();
                
                return values[n];
            }
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("{ ");
            for (int i = 0; i < keys.Length; i++)
            {
                s.Append(keys[i]);
                s.Append(" : ");
                s.Append(values[i]);
                if (i != keys.Length - 1)
                {
                    s.Append(", ");
                }
            }
            s.Append(" }");
            return s.ToString();
        }
    }
}