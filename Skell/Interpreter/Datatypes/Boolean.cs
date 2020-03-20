namespace Skell.Data
{
    public class Boolean : SkellData
    {
        bool value;

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

        public static Boolean operator !(Boolean a) => new Boolean(!a.value);

        public static Boolean operator &(Boolean a, Boolean b) => new Boolean(a.value & b.value);

        public static Boolean operator |(Boolean a, Boolean b) => new Boolean(a.value | b.value);

        public override string ToString() => value.ToString().ToLower();
    }
}