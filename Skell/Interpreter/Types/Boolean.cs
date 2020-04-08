namespace Skell.Types
{
    public class Boolean : ISkellType
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
            if (!bool.TryParse(token, out value)) {
                throw new Skell.Error.NativeParseFailure(token);
            }
        }

        public static bool operator >(Boolean a, Boolean b)
        {
            throw new Skell.Error.InvalidOperation(">", a, b);
        }

        public static bool operator >=(Boolean a, Boolean b)
        {
            throw new Skell.Error.InvalidOperation(">=", a, b);
        }

        public static bool operator <(Boolean a, Boolean b)
        {
            throw new Skell.Error.InvalidOperation("<", a, b);
        }

        public static bool operator <=(Boolean a, Boolean b)
        {
            throw new Skell.Error.InvalidOperation("<=", a, b);
        }

        public static bool operator ==(Boolean a, Boolean b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Boolean a, Boolean b)
        {
            return !a.Equals(b);
        }

        public static Boolean operator !(Boolean a) => new Boolean(!a.value);

        public override string ToString() => value.ToString().ToLower();

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Boolean b) {
                return value == b.value;
            }
            return false;
        }
    }
}