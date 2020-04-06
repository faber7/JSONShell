namespace Skell.Data
{
    // Base type for all datatypes in Skell
    public interface SkellData
    {
        
    }

    // Base type for any datatype that has member access support
    public interface SkellIndexableData : SkellData
    {
        int Count();

        SkellData GetMember(SkellData index);
    }
}