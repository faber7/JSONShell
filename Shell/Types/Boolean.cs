namespace Shell.Types
{
    public class Boolean : IShellData
    {
        public readonly bool value;

        public Boolean()
        {
            value = false;
        }

        public Boolean(bool val)
        {
            value = val;
        }

        public Boolean(string token)
        {
            value = bool.Parse(token);
        }

        public static Number operator +(Boolean a) => throw new Shell.Problems.InvalidOperation("+", a);
        public static Number operator -(Boolean a) => throw new Shell.Problems.InvalidOperation("-", a);
        public static Number operator +(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("+", a, b);
        public static Number operator -(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("-", a, b);
        public static Number operator *(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("*", a, b);
        public static Number operator /(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("/", a, b);

        public static Boolean operator !(Boolean a) => new Boolean(!a.value);
        public static Boolean operator ==(Boolean a, Boolean b) => new Boolean(a.Equals(b));
        public static Boolean operator !=(Boolean a, Boolean b) => new Boolean(!a.Equals(b));
        public static Boolean operator >(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation(">", a, b);
        public static Boolean operator >=(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation(">=", a, b);
        public static Boolean operator <(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("<", a, b);
        public static Boolean operator <=(Boolean a, Boolean b) => throw new Shell.Problems.InvalidOperation("<=", a, b);

        public override string ToString() => value.ToString().ToLower();
        public override int GetHashCode() => value.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is Boolean b)
                return value == b.value;

            return false;
        }
    }
}