namespace Skell.Types
{
    // Base type for all datatypes in Skell
    public interface ISkellType
    {
    }

    // Base type for any executable type
    public interface ISkellLambda : ISkellType
    {
    }

    // Base type for any datatype that has member access support
    public interface ISkellIndexableType : ISkellType
    {
        int Count();

        ISkellType GetMember(ISkellType index);
    }
}