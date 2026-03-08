namespace YuckQi.Data.Exceptions;

public sealed class RevisionException<TDomainEntity, TIdentifier> : ApplicationException
{
    public RevisionException(TIdentifier? identifier) : base(GetMessageText(identifier)) { }

    public RevisionException(TIdentifier? identifier, Exception inner) : base(GetMessageText(identifier), inner) { }

    private static String GetMessageText(TIdentifier? identifier)
    {
        return $"Failed to revise '{typeof(TDomainEntity).Name}' with identifier '{identifier}'.";
    }
}
