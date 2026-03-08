namespace YuckQi.Data.Exceptions;

public sealed class PhysicalDeletionException<TDomainEntity, TIdentifier> : ApplicationException
{
    public PhysicalDeletionException(TIdentifier? identifier) : base(GetMessageText(identifier)) { }

    public PhysicalDeletionException(TIdentifier? identifier, Exception inner) : base(GetMessageText(identifier), inner) { }

    private static String GetMessageText(TIdentifier? identifier)
    {
        return $"Failed to delete '{typeof(TDomainEntity).Name}' with identifier '{identifier}'.";
    }
}
