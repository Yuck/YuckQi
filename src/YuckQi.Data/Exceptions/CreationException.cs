namespace YuckQi.Data.Exceptions;

public sealed class CreationException<TDomainEntity> : ApplicationException
{
    public CreationException() : base(GetMessageText()) { }

    public CreationException(Exception inner) : base(GetMessageText(), inner) { }

    private static String GetMessageText()
    {
        return $"Failed to create '{typeof(TDomainEntity).Name}'.";
    }
}
