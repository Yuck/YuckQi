using System.Diagnostics.CodeAnalysis;

namespace YuckQi.Domain.Entities.Extensions;

public static class IdentifierExtensions
{
    public static Boolean IsSet<TIdentifier>([NotNullWhen(true)] this TIdentifier? identifier) where TIdentifier : IEquatable<TIdentifier>
    {
        if (identifier is null)
            return false;

        return ! EqualityComparer<TIdentifier>.Default.Equals(identifier, default);
    }
}
