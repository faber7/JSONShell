using System.Text;

namespace Skell.Data
{
    class Object : SkellData
    {
        private Skell.Data.String[] keys;
        private SkellData[] values;

        public Object(Skell.Data.String[] k, SkellData[] v)
        {
            if (k.Length != v.Length)
            {
                throw new System.NotImplementedException();
            }
            keys = k;
            values = v;
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