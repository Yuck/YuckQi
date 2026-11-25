using System.Reflection;

namespace YuckQi.Data.Sql.Dapper.Internal;

internal static class SimpleCrudScaffold<TRecord>
{
    private const String ListSeparator = ", ";
    private const String AssignmentSeparator = " = ";

    private static readonly PropertyInfo[] InsertablePropertiesOmittedBySimpleCrud = GetPropertiesOmittedBySimpleCrud(IsInsertable);
    private static readonly PropertyInfo[] UpdateablePropertiesOmittedBySimpleCrud = GetPropertiesOmittedBySimpleCrud(IsUpdateable);
    private static readonly Lock Gate = new();
    private static InsertFragments? _extendedInsertFragments;
    private static String? _extendedUpdateAssignments;

    public static void IncludeOmittedInsertColumns()
    {
        if (InsertablePropertiesOmittedBySimpleCrud.Length == 0 || IsInsertCacheExtended())
            return;

        lock (Gate)
        {
            var fragments = AppendOmittedInsertColumns(SimpleCrudInternals.RebuildInsertColumns<TRecord>(), SimpleCrudInternals.RebuildInsertValues<TRecord>());

            SimpleCrudInternals.SetInsertFragments<TRecord>(fragments);

            Volatile.Write(ref _extendedInsertFragments, fragments);
        }
    }

    public static void IncludeOmittedUpdateColumns(TRecord record)
    {
        if (UpdateablePropertiesOmittedBySimpleCrud.Length == 0 || IsUpdateCacheExtended())
            return;

        lock (Gate)
        {
            var assignments = AppendOmittedUpdateAssignments(SimpleCrudInternals.RebuildUpdateAssignments(record));

            SimpleCrudInternals.SetUpdateAssignments<TRecord>(assignments);

            Volatile.Write(ref _extendedUpdateAssignments, assignments);
        }
    }

    private static Boolean IsInsertCacheExtended()
    {
        var fragments = Volatile.Read(ref _extendedInsertFragments);

        return fragments is not null && SimpleCrudInternals.HasInsertFragments<TRecord>(fragments);
    }

    private static Boolean IsUpdateCacheExtended()
    {
        var assignments = Volatile.Read(ref _extendedUpdateAssignments);

        return assignments is not null && SimpleCrudInternals.HasUpdateAssignments<TRecord>(assignments);
    }

    private static InsertFragments AppendOmittedInsertColumns(String columns, String values)
    {
        var columnList = SplitList(columns);
        var valueList = SplitList(values);

        foreach (var property in InsertablePropertiesOmittedBySimpleCrud)
        {
            var column = SimpleCrudInternals.GetColumnName(property);

            if (columnList.Contains(column))
                continue;

            columnList.Add(column);
            valueList.Add($"@{property.Name}");
        }

        return new InsertFragments(JoinList(columnList), JoinList(valueList));
    }

    private static String AppendOmittedUpdateAssignments(String assignments)
    {
        var assignmentList = SplitList(assignments);
        var assignedColumns = assignmentList.Select(GetAssignedColumn).ToHashSet();

        foreach (var property in UpdateablePropertiesOmittedBySimpleCrud)
        {
            var column = SimpleCrudInternals.GetColumnName(property);

            if (assignedColumns.Add(column))
                assignmentList.Add($"{column}{AssignmentSeparator}@{property.Name}");
        }

        return JoinList(assignmentList);
    }

    private static String GetAssignedColumn(String assignment)
    {
        var separatorIndex = assignment.IndexOf(AssignmentSeparator, StringComparison.Ordinal);

        return separatorIndex < 0 ? assignment : assignment[..separatorIndex];
    }

    private static List<String> SplitList(String list)
    {
        return [.. list.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries)];
    }

    private static String JoinList(IEnumerable<String> items)
    {
        return String.Join(ListSeparator, items);
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
