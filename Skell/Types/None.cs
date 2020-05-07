namespace Skell.Types
{
    /// <summary>
    /// Cannot be constructed in Skell code, only used as a return value
    /// </summary>
    public class None : ISkellReturnable
    {
        public override string ToString() => "";
    }
}