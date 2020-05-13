namespace Shell.Types
{
    /// <summary>
    /// Cannot be constructed in Shell code, only used as a return value
    /// </summary>
    public class None : IShellReturnable
    {
        public override string ToString() => "";
    }
}