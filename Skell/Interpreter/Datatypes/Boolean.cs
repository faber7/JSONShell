namespace Skell.Data
{
    public class Boolean : ISkellData
    {
        public readonly bool value;

        public Boolean(bool val)
        {
            value = val;
        }

        public Boolean(string token)
        {
            if (!bool.TryParse(token, out value)) {
                throw new Skell.Error.NativeParseError(token);
            }
        }

        public static bool operator >(Boolean a, Boolean b)
        {
            throw new System.NotImplementedException();
        }

        public static bool operator >=(Boolean a, Boolean b)
        {
            throw new System.NotImplementedException();
        }

        public static bool operator <(Boolean a, Boolean b)
        {
            throw new System.NotImplementedException();
        }

        public static bool operator <=(Boolean a, Boolean b)
        {
            throw new System.NotImplementedException();
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

        public static Boolean operator &(Boolean a, Boolean b) => new Boolean(a.value & b.value);

        public static Boolean operator |(Boolean a, Boolean b) => new Boolean(a.value | b.value);

        public override string ToString() => value.ToString().ToLower();

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Boolean b) {
                return value == b.value;
            }
            throw new System.NotImplementedException();
        }
    }
}