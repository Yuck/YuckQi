using System.Reflection;

namespace YuckQi.Data.Sql.Dapper.Internal;

internal static class SimpleCrudScaffold<TRecord>
{
    private static readonly PropertyInfo[] InsertablePropertiesOmittedBySimpleCrud = GetPropertiesOmittedBySimpleCrud(IsInsertable);
    private static readonly PropertyInfo[] UpdateablePropertiesOmittedBySimpleCrud = GetPropertiesOmittedBySimpleCrud(IsUpdateable);
    private static readonly Lock Gate = new();
    private static Boolean _hasIncludedInsertColumns;
    private static Boolean _hasIncludedUpdateColumns;

    public static void IncludeOmittedInsertColumns()
    {
        if (Volatile.Read(ref _hasIncludedInsertColumns))
            return;

        lock (Gate)
        {
            if (_hasIncludedInsertColumns)
                return;

            if (InsertablePropertiesOmittedBySimpleCrud.Length > 0)
            {
                var columns = InsertablePropertiesOmittedBySimpleCrud.Select(SimpleCrudInternals.GetColumnName);
                var values = InsertablePropertiesOmittedBySimpleCrud.Select(t => $"@{t.Name}");

                SimpleCrudInternals.SetInsertColumns<TRecord>(AppendWithoutDuplicates(SimpleCrudInternals.GetInsertColumns<TRecord>(), columns));
                SimpleCrudInternals.SetInsertValues<TRecord>(AppendWithoutDuplicates(SimpleCrudInternals.GetInsertValues<TRecord>(), values));
            }

            Volatile.Write(ref _hasIncludedInsertColumns, true);
        }
    }

    public static void IncludeOmittedUpdateColumns(TRecord record)
    {
        if (Volatile.Read(ref _hasIncludedUpdateColumns))
            return;

        lock (Gate)
        {
            if (_hasIncludedUpdateColumns)
                return;

            if (UpdateablePropertiesOmittedBySimpleCrud.Length > 0)
            {
                var assignments = UpdateablePropertiesOmittedBySimpleCrud.Select(t => $"{SimpleCrudInternals.GetColumnName(t)} = @{t.Name}");

                SimpleCrudInternals.SetUpdateAssignments<TRecord>(AppendWithoutDuplicates(SimpleCrudInternals.GetUpdateAssignments(record), assignments));
            }

            Volatile.Write(ref _hasIncludedUpdateColumns, true);
        }
    }

    private static String AppendWithoutDuplicates(String commaSeparatedList, IEnumerable<String> items)
    {
        var list = commaSeparatedList.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();

        list.AddRange(items.Where(t => ! list.Contains(t)).ToList());

        return String.Join(", ", list);
    }

    private static PropertyInfo[] GetPropertiesOmittedBySimpleCrud(Func<PropertyInfo, Boolean> isWritable)
    {
        return typeof(TRecord).GetProperties()
                              .Where(t => IsDateOnlyOrTimeOnly(t.PropertyType) && ! HasAttributeNamed(t, "EditableAttribute"))
                              .Where(isWritable)
                              .ToArray();
    }

    private static Boolean IsInsertable(PropertyInfo property)
    {
        if (HasAttributeNamed(property, "KeyAttribute") && ! HasAttributeNamed(property, "RequiredAttribute"))
            return false;
        if (HasAttributeNamed(property, "IgnoreInsertAttribute") || HasAttributeNamed(property, "NotMappedAttribute") || IsMarkedReadOnly(property))
            return false;
        if (IsNamedId(property) && ! HasAttributeNamed(property, "RequiredAttribute"))
            return false;

        return true;
    }

    private static Boolean IsUpdateable(PropertyInfo property)
    {
        if (IsNamedId(property) || HasAttributeNamed(property, "KeyAttribute"))
            return false;
        if (HasAttributeNamed(property, "IgnoreUpdateAttribute") || HasAttributeNamed(property, "NotMappedAttribute") || IsMarkedReadOnly(property))
            return false;

        return true;
    }

    private static Boolean IsDateOnlyOrTimeOnly(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying == typeof(DateOnly) || underlying == typeof(TimeOnly);
    }

    private static Boolean IsNamedId(PropertyInfo property)
    {
        return property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase);
    }

    private static Boolean HasAttributeNamed(PropertyInfo property, String name)
    {
        return property.GetCustomAttributes(true).Any(t => t.GetType().Name == name);
    }

    private static Boolean IsMarkedReadOnly(PropertyInfo property)
    {
        var attribute = property.GetCustomAttributes(true).FirstOrDefault(t => t.GetType().Name == "ReadOnlyAttribute");

        return attribute?.GetType().GetProperty("IsReadOnly")?.GetValue(attribute) is true;
    }
}
