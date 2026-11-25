using System.Reflection;

namespace YuckQi.Data.Sql.Dapper.Schema;

internal static class NamedAttributes
{
    public static Attribute? Find(MemberInfo member, Type preferred)
    {
        return GetMatches(member, preferred).OrderBy(t => t.GetType() == preferred ? 0 : 1).ThenBy(t => t.GetType().FullName, StringComparer.Ordinal).FirstOrDefault();
    }

    public static String? GetText(Attribute? attribute, String propertyName)
    {
        return attribute?.GetType().GetProperty(propertyName)?.GetValue(attribute) as String;
    }

    public static Boolean IsDefined(MemberInfo member, Type preferred)
    {
        return GetMatches(member, preferred).Any();
    }

    private static IEnumerable<Attribute> GetMatches(MemberInfo member, Type preferred)
    {
        return member.GetCustomAttributes(true).OfType<Attribute>().Where(t => t.GetType().Name == preferred.Name);
    }
}
