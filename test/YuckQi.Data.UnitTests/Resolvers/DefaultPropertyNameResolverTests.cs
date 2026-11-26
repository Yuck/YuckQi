using System.Linq.Expressions;
using NUnit.Framework;
using YuckQi.Data.Resolvers;

namespace YuckQi.Data.UnitTests.Resolvers;

public class DefaultPropertyNameResolverTests
{
    [Test]
    public void Resolve_WithMember_ReturnsName()
    {
        var path = new DefaultPropertyNameResolver().Resolve<TestDomainEntity, TestDomainEntity>(t => t.Name);

        Assert.That(path, Is.EqualTo("Name"));
    }

    [Test]
    public void Resolve_WithBoxedValueMember_ReturnsName()
    {
        var path = new DefaultPropertyNameResolver().Resolve<TestDomainEntity, TestDomainEntity>(t => t.Identifier);

        Assert.That(path, Is.EqualTo("Identifier"));
    }

    [Test]
    public void Resolve_WithNestedMember_ReturnsDottedPath()
    {
        var path = new DefaultPropertyNameResolver().Resolve<TestDomainEntity, TestDomainEntity>(t => t.Address.City);

        Assert.That(path, Is.EqualTo("Address.City"));
    }

    [Test]
    public void Resolve_WithCastInsideChain_ReturnsFullPath()
    {
        var path = new DefaultPropertyNameResolver().Resolve<TestDomainEntity, TestDomainEntity>(t => ((TestDerivedAddress) t.Address).Region);

        Assert.That(path, Is.EqualTo("Address.Region"));
    }

    [Test]
    public void Resolve_WithValueOfNullableMember_ReturnsNullableMemberName()
    {
        var parameter = Expression.Parameter(typeof(TestDomainEntity), "t");
        var member = Expression.Property(Expression.Property(parameter, nameof(TestDomainEntity.Age)), nameof(Nullable<Int32>.Value));
        var expression = Expression.Lambda<Func<TestDomainEntity, Object>>(Expression.Convert(member, typeof(Object)), parameter);

        var path = new DefaultPropertyNameResolver().Resolve<TestDomainEntity, TestDomainEntity>(expression);

        Assert.That(path, Is.EqualTo("Age"));
    }

    [Test]
    public void Resolve_WithHasValueOfNullableMember_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => t.Age.HasValue), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resolve_WithMethodCall_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => t.Name.ToLower()), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resolve_WithMemberOfMethodCall_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => t.Name.Trim().Length), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resolve_WithParameter_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => t), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resolve_WithCapturedMember_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();
        var other = new TestDomainEntity();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => other.Name), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resolve_WithStaticMember_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.Resolve<TestDomainEntity, TestDomainEntity>(t => String.Empty), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void WithMapping_WithUnsupportedPath_ThrowsNotSupportedException()
    {
        var resolver = new DefaultPropertyNameResolver();

        Assert.That(() => resolver.WithMapping<TestDomainEntity, TestDomainEntity>(t => t.Name.ToUpper(), t => t.Name), Throws.InstanceOf<NotSupportedException>());
    }

    private class TestAddress
    {
        public String City { get; set; } = String.Empty;
    }

    private sealed class TestDerivedAddress : TestAddress
    {
        public String Region { get; set; } = String.Empty;
    }

    private sealed class TestDomainEntity
    {
        public TestAddress Address { get; set; } = new();

        public Int32? Age { get; set; }

        public Int32 Identifier { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
