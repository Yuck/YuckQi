namespace YuckQi.Data.DocumentDb.RavenDb.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute(String name) : Attribute
{
    public String Name { get; } = name;
}
