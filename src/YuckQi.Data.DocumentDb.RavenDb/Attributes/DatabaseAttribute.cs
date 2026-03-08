namespace YuckQi.Data.DocumentDb.RavenDb.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DatabaseAttribute(String name) : Attribute
{
    public String Name { get; } = name;
}
