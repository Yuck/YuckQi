using System.Reflection;
using Dapper;

namespace YuckQi.Data.Sql.Dapper.Schema;

public static class RecordSchema<TRecord>
{
    private static readonly Attribute? Table = NamedAttributes.Find(typeof(TRecord), typeof(TableAttribute));
    private static readonly HashSet<String> ConstructorParameterNames = GetConstructorParameterNames();
    private static readonly PropertyInfo[] MappedProperties = typeof(TRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(IsMapped).ToArray();
    private static readonly MappedColumn[] MappedColumns = MappedProperties.Select(t => new MappedColumn(t, GetColumnName(t))).ToArray();

    public static IReadOnlyList<PropertyInfo> KeyProperties { get; } = GetKeyProperties();

    public static String? SchemaName { get; } = NullIfBlank(NamedAttributes.GetText(Table, nameof(TableAttribute.Schema)));

    public static IReadOnlyList<PropertyInfo> SelectableProperties { get; } = MappedProperties.Where(t => ! NamedAttributes.IsDefined(t, typeof(IgnoreSelectAttribute))).ToArray();

    public static String TableName { get; } = NullIfBlank(NamedAttributes.GetText(Table, nameof(TableAttribute.Name))) ?? typeof(TRecord).Name;

    public static String GetColumnName(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var column = NamedAttributes.Find(property, typeof(ColumnAttribute));

        return NullIfBlank(NamedAttributes.GetText(column, nameof(ColumnAttribute.Name))) ?? property.Name;
    }

    public static String GetColumnName(String fieldName)
    {
        return GetColumnName(GetProperty(fieldName));
    }

    public static PropertyInfo GetProperty(String fieldName)
    {
        ArgumentNullException.ThrowIfNull(fieldName);

        var byPropertyName = FindSingle(t => String.Equals(t.Property.Name, fieldName, StringComparison.Ordinal));
        if (byPropertyName is not null)
            return byPropertyName;

        var byColumnName = FindSingle(t => String.Equals(t.Column, fieldName, StringComparison.Ordinal));
        if (byColumnName is not null)
            return byColumnName;

        var byPropertyNameIgnoringCase = FindSingle(t => String.Equals(t.Property.Name, fieldName, StringComparison.OrdinalIgnoreCase));
        if (byPropertyNameIgnoringCase is not null)
            return byPropertyNameIgnoringCase;

        var byColumnNameIgnoringCase = FindSingle(t => String.Equals(t.Column, fieldName, StringComparison.OrdinalIgnoreCase));
        if (byColumnNameIgnoringCase is not null)
            return byColumnNameIgnoringCase;

        throw new ArgumentException($"'{fieldName}' does not identify exactly one mapped property or column of {typeof(TRecord).Name}.", nameof(fieldName));
    }

    private static PropertyInfo? FindSingle(Func<MappedColumn, Boolean> predicate)
    {
        var matches = MappedColumns.Where(predicate).Take(2).ToArray();

        return matches.Length == 1 ? matches[0].Property : null;
    }

    private static HashSet<String> GetConstructorParameterNames()
    {
        var constructors = typeof(TRecord).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var names = constructors.SelectMany(t => t.GetParameters()).Select(t => t.Name).OfType<String>();

        return names.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static PropertyInfo[] GetKeyProperties()
    {
        var keyed = MappedProperties.Where(t => NamedAttributes.IsDefined(t, typeof(KeyAttribute))).ToArray();
        if (keyed.Length != 0)
            return keyed;

        return MappedProperties.Where(t => String.Equals(t.Name, "Id", StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private static Boolean HasAutoPropertyBackingField(PropertyInfo property)
    {
        return property.DeclaringType?.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) is not null;
    }

    private static Boolean IsMapped(PropertyInfo property)
    {
        if (! property.CanRead || property.GetIndexParameters().Length != 0)
            return false;

        if (NamedAttributes.IsDefined(property, typeof(NotMappedAttribute)))
            return false;

        return property.CanWrite || HasAutoPropertyBackingField(property) || ConstructorParameterNames.Contains(property.Name);
    }

    private static String? NullIfBlank(String? value)
    {
        return String.IsNullOrWhiteSpace(value) ? null : value;
    }
}
