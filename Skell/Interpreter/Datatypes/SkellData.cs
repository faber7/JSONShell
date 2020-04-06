namespace Skell.Data
{
    // Base type for all datatypes in Skell
    public interface ISkellData
    {
        
    }

    // Base type for any datatype that has member access support
    public interface ISkellIndexableData : ISkellData
    {
        int Count();

        ISkellData GetMember(ISkellData index);
    }
}