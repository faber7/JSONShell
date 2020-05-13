using System.Text;

namespace Shell.Types
{
    public class String : IShellData
    {
        public readonly string contents;

        public String()
        {
            contents = "";
        }

        public String(string token)
        {
            contents = token;
        }

        public Number Length() => new Number(contents.Length);

        public String Substring(Number start, Number stop) => new String(contents[start.integerValue..stop.integerValue]);

        public static Number operator +(String a) => throw new Shell.Problems.InvalidOperation("+", a);
        public static Number operator -(String a) => throw new Shell.Problems.InvalidOperation("-", a);
        public static Number operator +(String a, String b) => throw new Shell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(String a, String b) => throw new Shell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(String a, String b) => throw new Shell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(String a, String b) => throw new Shell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(String a) => throw new Shell.Problems.InvalidOperation("!", a);
        public static Boolean operator ==(String a1, String a2) => new Boolean(a1.Equals(a2));
        public static Boolean operator !=(String a1, String a2) => new Boolean(!a1.Equals(a2));
        public static Boolean operator >(String a, String b) => throw new Shell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(String a, String b) => throw new Shell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(String a, String b) => throw new Shell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(String a, String b) => throw new Shell.Problems.InvalidOperation("<=", a, b);

        public override string ToString()
        {
            return $"\"{contents}\"";
        }
        public override int GetHashCode() => contents.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is String s)
                return contents == s.contents;
                
            return false;
        }
    }
}