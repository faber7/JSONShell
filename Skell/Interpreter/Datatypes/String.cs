using System.Text;

namespace Skell.Data
{
    class String : SkellData
    {
        public readonly string contents;

        public String(string token)
        {
            contents = token;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("\"");
            s.Append(contents);
            s.Append("\"");
            return s.ToString();
        }

        public static bool operator ==(String s1, String s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(String s1, String s2)
        {
            return !s1.Equals(s2);
        }

        public override int GetHashCode() => contents.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is String s) {
                return contents == s.contents;
            }
            throw new System.NotImplementedException();
        }
    }
}