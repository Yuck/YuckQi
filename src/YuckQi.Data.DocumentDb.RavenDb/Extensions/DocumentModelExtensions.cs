using System.Collections.Concurrent;
using System.Reflection;
using YuckQi.Data.DocumentDb.RavenDb.Attributes;

namespace YuckQi.Data.DocumentDb.RavenDb.Extensions;

public static class DocumentModelExtensions
{
    private const String DefaultIdPropertyName = "Id";

    private static readonly ConcurrentDictionary<Type, String> CollectionNameByType = new ();
    private static readonly ConcurrentDictionary<Type, String> DatabaseNameByType = new ();
    private static readonly ConcurrentDictionary<Type, PropertyInfo> IdentifierByType = new ();

    public static String? GetCollectionName(this Type? type)
    {
        return type != null ? CollectionNameByType.GetOrAdd(type, t => GetCollectionAttribute(t)?.Name ?? t.Name) : null;
    }

    public static String? GetDatabaseName(this Type? type)
    {
        return type != null ? DatabaseNameByType.GetOrAdd(type, t => GetDatabaseAttribute(t).Name) : null;
    }

    public static TIdentifier? GetIdentifier<TDocument, TIdentifier>(this TDocument document)
    {
        if (document == null)
            return default;

        var property = GetIdentifierPropertyInfo(typeof(TDocument)) ?? throw new InvalidOperationException($"{typeof(TDocument).Name} identifier property could not be determined.");
        var value = property.GetValue(document);
        if (value is TIdentifier identifier)
            return identifier;

        throw new InvalidOperationException($"{typeof(TDocument).Name} identifier type {property.PropertyType.Name} does not match declared type {typeof(TIdentifier).Name}.");
    }

    public static String ToDocumentId<TIdentifier>(this TIdentifier? identifier) where TIdentifier : IEquatable<TIdentifier>
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));

        return identifier.ToString() ?? throw new InvalidOperationException("Identifier string representation cannot be null.");
    }

    public static void SetDocumentId<TDocument, TIdentifier>(this TDocument document, TIdentifier? identifier) where TIdentifier : IEquatable<TIdentifier>
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));

        var property = GetIdentifierPropertyInfo(typeof(TDocument)) ?? throw new InvalidOperationException($"{typeof(TDocument).Name} identifier property could not be determined.");

        property.SetValue(document, identifier.ToDocumentId());
    }

    private static CollectionAttribute? GetCollectionAttribute(MemberInfo type)
    {
        return type.GetCustomAttribute(typeof(CollectionAttribute)) as CollectionAttribute;
    }

    private static DatabaseAttribute GetDatabaseAttribute(MemberInfo type)
    {
        if (type.GetCustomAttribute(typeof(DatabaseAttribute)) is DatabaseAttribute attribute)
            return attribute;

        throw new InvalidOperationException($"Type '{type.Name}' is missing the required [{nameof(DatabaseAttribute)}].");
    }

    private static PropertyInfo? GetIdentifierPropertyInfo(Type? type)
    {
        return type != null ? IdentifierByType.GetOrAdd(type, IdentifierPropertyInfoValueFactory) : null;
    }

    private static PropertyInfo IdentifierPropertyInfoValueFactory(Type type)
    {
        var property = type.GetProperty(DefaultIdPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
            return property;

        throw new InvalidOperationException($"Type '{type.Name}' does not have an identifier property. Define a property named '{DefaultIdPropertyName}'.");
    }
}
