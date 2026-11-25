using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Dapper;

namespace YuckQi.Data.Sql.Dapper.Internal;

internal static class SimpleCrudInternals
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

    private static readonly MethodInfo BuildInsertParameters = GetPrivateMethod(nameof(BuildInsertParameters));
    private static readonly MethodInfo BuildInsertValues = GetPrivateMethod(nameof(BuildInsertValues));
    private static readonly MethodInfo BuildUpdateSet = GetPrivateMethod(nameof(BuildUpdateSet));
    private static readonly Func<PropertyInfo, String> SimpleCrudGetColumnName = GetPrivateMethod("GetColumnName").CreateDelegate<Func<PropertyInfo, String>>();
    private static readonly ConcurrentDictionary<String, String> StringBuilderCache = GetStringBuilderCache();

    public static String GetColumnName(PropertyInfo property)
    {
        return SimpleCrudGetColumnName(property);
    }

    public static String GetInsertColumns<TRecord>()
    {
        return InvokeBuilder<TRecord>(BuildInsertParameters);
    }

    public static String GetInsertValues<TRecord>()
    {
        return InvokeBuilder<TRecord>(BuildInsertValues);
    }

    public static String GetUpdateAssignments<TRecord>(TRecord record)
    {
        return InvokeBuilder<TRecord>(BuildUpdateSet, record);
    }

    public static void SetInsertColumns<TRecord>(String columns)
    {
        StringBuilderCache[GetCacheKey<TRecord>(BuildInsertParameters)] = columns;
    }

    public static void SetInsertValues<TRecord>(String values)
    {
        StringBuilderCache[GetCacheKey<TRecord>(BuildInsertValues)] = values;
    }

    public static void SetUpdateAssignments<TRecord>(String assignments)
    {
        StringBuilderCache[GetCacheKey<TRecord>(BuildUpdateSet)] = assignments;
    }

    private static String GetCacheKey<TRecord>(MethodInfo builder)
    {
        return $"{typeof(TRecord).FullName}_{builder.Name}";
    }

    private static MethodInfo GetPrivateMethod(String name)
    {
        return typeof(SimpleCRUD).GetMethod(name, PrivateStatic) ?? throw CreateUnsupportedVersionException(name);
    }

    private static ConcurrentDictionary<String, String> GetStringBuilderCache()
    {
        const String name = "StringBuilderCacheDict";

        var field = typeof(SimpleCRUD).GetField(name, PrivateStatic);

        return field?.GetValue(null) as ConcurrentDictionary<String, String> ?? throw CreateUnsupportedVersionException(name);
    }

    private static InvalidOperationException CreateUnsupportedVersionException(String member)
    {
        var version = typeof(SimpleCRUD).Assembly.GetName().Version;

        return new InvalidOperationException($"Dapper.SimpleCRUD {version} does not expose the internal member '{member}' that YuckQi.Data.Sql.Dapper uses to include DateOnly/TimeOnly columns in inserts and updates. This SimpleCRUD version is not supported.");
    }

    private static String InvokeBuilder<TRecord>(MethodInfo builder, params Object?[] argumentsBeforeStringBuilder)
    {
        var stringBuilder = new StringBuilder();

        builder.MakeGenericMethod(typeof(TRecord)).Invoke(null, [.. argumentsBeforeStringBuilder, stringBuilder]);

        return stringBuilder.ToString();
    }
}
