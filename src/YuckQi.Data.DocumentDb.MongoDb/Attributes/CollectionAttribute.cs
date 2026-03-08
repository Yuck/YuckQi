namespace YuckQi.Data.DocumentDb.MongoDb.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute(String name) : Attribute
{
    public String Name { get; } = name;
}
