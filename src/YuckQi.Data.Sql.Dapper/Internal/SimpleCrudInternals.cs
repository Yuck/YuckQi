using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Dapper;

namespace YuckQi.Data.Sql.Dapper.Internal;

internal static class SimpleCrudInternals
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const String BuildInsertParametersName = "BuildInsertParameters";
    private const String BuildInsertValuesName = "BuildInsertValues";
    private const String BuildUpdateSetName = "BuildUpdateSet";
    private const String GetColumnNameName = "GetColumnName";
    private const String StringBuilderCacheName = "StringBuilderCacheDict";
    private const String UpdateStatementName = "Update";

    private static readonly Lazy<MethodInfo> BuildInsertParameters = new(() => GetPrivateMethod(BuildInsertParametersName));
    private static readonly Lazy<MethodInfo> BuildInsertValues = new(() => GetPrivateMethod(BuildInsertValuesName));
    private static readonly Lazy<MethodInfo> BuildUpdateSet = new(() => GetPrivateMethod(BuildUpdateSetName));
    private static readonly Lazy<Func<PropertyInfo, String>> SimpleCrudGetColumnName = new(CreateGetColumnNameDelegate);
    private static readonly Lazy<ConcurrentDictionary<String, String>> StringBuilderCache = new(GetStringBuilderCache);

    public static String GetColumnName(PropertyInfo property)
    {
        return SimpleCrudGetColumnName.Value(property);
    }

    public static String RebuildInsertColumns<TRecord>()
    {
        return Rebuild<TRecord>(BuildInsertParameters);
    }

    public static String RebuildInsertValues<TRecord>()
    {
        return Rebuild<TRecord>(BuildInsertValues);
    }

    public static String RebuildUpdateAssignments<TRecord>(TRecord record)
    {
        return Rebuild<TRecord>(BuildUpdateSet, record);
    }

    public static Boolean HasInsertFragments<TRecord>(InsertFragments fragments)
    {
        return IsCached<TRecord>(BuildInsertParametersName, fragments.Columns) && IsCached<TRecord>(BuildInsertValuesName, fragments.Values);
    }

    public static Boolean HasUpdateAssignments<TRecord>(String assignments)
    {
        return IsCached<TRecord>(BuildUpdateSetName, assignments) && ! IsUpdateStatementStale<TRecord>(assignments);
    }

    public static void SetInsertFragments<TRecord>(InsertFragments fragments)
    {
        StringBuilderCache.Value[GetCacheKey<TRecord>(BuildInsertValuesName)] = fragments.Values;
        StringBuilderCache.Value[GetCacheKey<TRecord>(BuildInsertParametersName)] = fragments.Columns;
    }

    public static void SetUpdateAssignments<TRecord>(String assignments)
    {
        StringBuilderCache.Value[GetCacheKey<TRecord>(BuildUpdateSetName)] = assignments;
        StringBuilderCache.Value.TryRemove(GetCacheKey<TRecord>(UpdateStatementName), out _);
    }

    private static Func<PropertyInfo, String> CreateGetColumnNameDelegate()
    {
        try
        {
            return GetPrivateMethod(GetColumnNameName).CreateDelegate<Func<PropertyInfo, String>>();
        }
        catch (ArgumentException)
        {
            throw CreateUnsupportedVersionException(GetColumnNameName);
        }
    }

    private static InvalidOperationException CreateUnsupportedVersionException(String member)
    {
        var version = typeof(SimpleCRUD).Assembly.GetName().Version;

        return new InvalidOperationException($"Dapper.SimpleCRUD {version} does not expose the internal member '{member}' that YuckQi.Data.Sql.Dapper uses to include DateOnly/TimeOnly columns in inserts and updates. This SimpleCRUD version is not supported.");
    }

    private static String Rebuild<TRecord>(Lazy<MethodInfo> builder, params Object?[] argumentsBeforeStringBuilder)
    {
        StringBuilderCache.Value.TryRemove(GetCacheKey<TRecord>(builder.Value.Name), out _);

        var stringBuilder = new StringBuilder();

        builder.Value.MakeGenericMethod(typeof(TRecord)).Invoke(null, [.. argumentsBeforeStringBuilder, stringBuilder]);

        return stringBuilder.ToString();
    }

    private static String GetCacheKey<TRecord>(String name)
    {
        return $"{typeof(TRecord).FullName}_{name}";
    }

    private static MethodInfo GetPrivateMethod(String name)
    {
        return typeof(SimpleCRUD).GetMethod(name, PrivateStatic) ?? throw CreateUnsupportedVersionException(name);
    }

    private static ConcurrentDictionary<String, String> GetStringBuilderCache()
    {
        var field = typeof(SimpleCRUD).GetField(StringBuilderCacheName, PrivateStatic);

        return field?.GetValue(null) as ConcurrentDictionary<String, String> ?? throw CreateUnsupportedVersionException(StringBuilderCacheName);
    }

    private static Boolean IsCached<TRecord>(String name, String expected)
    {
        return StringBuilderCache.Value.TryGetValue(GetCacheKey<TRecord>(name), out var cached) && String.Equals(cached, expected, StringComparison.Ordinal);
    }

    private static Boolean IsUpdateStatementStale<TRecord>(String assignments)
    {
        return StringBuilderCache.Value.TryGetValue(GetCacheKey<TRecord>(UpdateStatementName), out var statement) && ! statement.Contains($" set {assignments} where ", StringComparison.Ordinal);
    }
}
