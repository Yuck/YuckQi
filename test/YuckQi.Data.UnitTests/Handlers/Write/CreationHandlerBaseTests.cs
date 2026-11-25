using NUnit.Framework;
using YuckQi.Data.Exceptions;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.UnitTests.Handlers.Write;

public class CreationHandlerBaseTests
{
    [Test]
    public void Create_WhenProviderReturnsDefaultGuid_ThrowsCreationException()
    {
        var handler = new StubCreationHandler<GuidEntity, Guid>(Guid.Empty);
        var entity = new GuidEntity();

        Assert.Throws<CreationException<GuidEntity>>(() => handler.Create(entity, new Object()));
    }

    [Test]
    public void Create_WhenProviderReturnsDefaultInt32_ThrowsCreationException()
    {
        var handler = new StubCreationHandler<Int32Entity, Int32>(0);
        var entity = new Int32Entity();

        Assert.Throws<CreationException<Int32Entity>>(() => handler.Create(entity, new Object()));
    }

    [Test]
    public void Create_WhenProviderReturnsDefaultInt32WithAllowDefaultIdentifier_AssignsIdentifier()
    {
        var handler = new StubCreationHandler<Int32Entity, Int32>(0, new CreationOptions<Int32>(allowDefaultIdentifier: true));
        var entity = new Int32Entity { Identifier = 7 };

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.Zero);
    }

    [Test]
    public void Create_WhenProviderReturnsNullStringWithAllowDefaultIdentifier_ThrowsCreationException()
    {
        var handler = new StubCreationHandler<StringEntity, String>(null, new CreationOptions<String>(allowDefaultIdentifier: true));
        var entity = new StringEntity();

        Assert.Throws<CreationException<StringEntity>>(() => handler.Create(entity, new Object()));
    }

    [Test]
    public void Create_WhenProviderReturnsDefaultInt32_LeavesIdentifierUnchanged()
    {
        var handler = new StubCreationHandler<Int32Entity, Int32>(0);
        var entity = new Int32Entity { Identifier = 7 };

        Assert.Throws<CreationException<Int32Entity>>(() => handler.Create(entity, new Object()));
        Assert.That(entity.Identifier, Is.EqualTo(7));
    }

    [Test]
    public void Create_WhenProviderReturnsNullString_ThrowsCreationException()
    {
        var handler = new StubCreationHandler<StringEntity, String>(null);
        var entity = new StringEntity();

        Assert.Throws<CreationException<StringEntity>>(() => handler.Create(entity, new Object()));
    }

    [Test]
    public void Create_WhenProviderReturnsIdentifier_AssignsIdentifier()
    {
        var handler = new StubCreationHandler<Int32Entity, Int32>(42);
        var entity = new Int32Entity();

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo(42));
    }

    [Test]
    public async Task Create_Async_WhenProviderReturnsDefaultInt32_ThrowsCreationException()
    {
        var handler = new StubCreationHandler<Int32Entity, Int32>(0);
        var entity = new Int32Entity();

        await Assert.ThrowsAsync<CreationException<Int32Entity>>(async () => await handler.Create(entity, new Object(), CancellationToken.None));
    }

    [Test]
    public async Task Create_Async_WhenProviderReturnsIdentifier_AssignsIdentifier()
    {
        var identifier = Guid.NewGuid();
        var handler = new StubCreationHandler<GuidEntity, Guid>(identifier);
        var entity = new GuidEntity();

        var created = await handler.Create(entity, new Object(), CancellationToken.None);

        Assert.That(created.Identifier, Is.EqualTo(identifier));
    }

    [Test]
    public void Create_Batch_WritesEachEntityOnceWithoutEnumeration()
    {
        var handler = new CountingCreationHandler();
        var entities = new[] { new Int32Entity(), new Int32Entity() };

        handler.Create(entities, new Object());

        Assert.That(handler.Calls, Is.EqualTo(2));
    }

    [Test]
    public void Create_Batch_WritesEachEntityOnceWhenEnumeratedRepeatedly()
    {
        var handler = new CountingCreationHandler();
        var entities = new[] { new Int32Entity(), new Int32Entity() };

        var created = handler.Create(entities, new Object());
        var identifiers = created.Select(entity => entity.Identifier).ToList();
        var again = created.Select(entity => entity.Identifier).ToList();

        Assert.That(handler.Calls, Is.EqualTo(2));
        Assert.That(identifiers, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(again, Is.EqualTo(identifiers));
    }

    [Test]
    public async Task Create_Async_Batch_WritesEntitiesSequentially()
    {
        var handler = new CountingCreationHandler();
        var entities = new[] { new Int32Entity(), new Int32Entity(), new Int32Entity() };

        var created = (await handler.Create(entities, new Object(), CancellationToken.None)).ToList();

        Assert.That(handler.Calls, Is.EqualTo(3));
        Assert.That(handler.MaximumConcurrency, Is.EqualTo(1));
        Assert.That(created.Select(entity => entity.Identifier), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Create_Batch_WhenScopeIsNull_ThrowsArgumentNullException()
    {
        var handler = new CountingCreationHandler();
        var entities = new[] { new Int32Entity() };

        Assert.Throws<ArgumentNullException>(() => handler.Create(entities, null));
        Assert.That(handler.Calls, Is.Zero);
    }

    [Test]
    public void Create_WhenIdentifierFactorySetAndIdentifierSet_PreservesIdentifier()
    {
        var identifier = Guid.NewGuid();
        var factory = Guid.NewGuid();
        var handler = new EchoCreationHandler<GuidEntity, Guid>(new CreationOptions<Guid>(() => factory));
        var entity = new GuidEntity { Identifier = identifier };

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo(identifier));
    }

    [Test]
    public void Create_WhenIdentifierFactorySetAndIdentifierUnset_AssignsFactoryIdentifier()
    {
        var factory = Guid.NewGuid();
        var handler = new EchoCreationHandler<GuidEntity, Guid>(new CreationOptions<Guid>(() => factory));
        var entity = new GuidEntity();

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo(factory));
    }

    [Test]
    public void Create_WhenIdentifierFactorySetAndStringIdentifierSet_PreservesIdentifier()
    {
        var handler = new EchoCreationHandler<StringEntity, String>(new CreationOptions<String>(() => "generated"));
        var entity = new StringEntity { Identifier = "explicit" };

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo("explicit"));
    }

    [Test]
    public void Create_WhenIdentifierFactorySetAndStringIdentifierNull_AssignsFactoryIdentifier()
    {
        var handler = new EchoCreationHandler<StringEntity, String>(new CreationOptions<String>(() => "generated"));
        var entity = new StringEntity();

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo("generated"));
    }

    [Test]
    public void Create_WhenIdentifierFactorySetWithAllowDefaultIdentifierAndIdentifierDefault_AssignsFactoryIdentifier()
    {
        var handler = new EchoCreationHandler<Int32Entity, Int32>(new CreationOptions<Int32>(() => 99, allowDefaultIdentifier: true));
        var entity = new Int32Entity();

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo(99));
    }

    [Test]
    public void Create_WhenIdentifierFactorySetWithAllowDefaultIdentifierAndIdentifierSet_PreservesIdentifier()
    {
        var handler = new EchoCreationHandler<Int32Entity, Int32>(new CreationOptions<Int32>(() => 99, allowDefaultIdentifier: true));
        var entity = new Int32Entity { Identifier = 7 };

        var created = handler.Create(entity, new Object());

        Assert.That(created.Identifier, Is.EqualTo(7));
    }

    [Test]
    public async Task Create_Async_Batch_WhenIdentifierFactorySet_PreservesExplicitIdentifiers()
    {
        var explicitIdentifier = Guid.NewGuid();
        var factory = Guid.NewGuid();
        var handler = new EchoCreationHandler<GuidEntity, Guid>(new CreationOptions<Guid>(() => factory));
        var entities = new[] { new GuidEntity { Identifier = explicitIdentifier }, new GuidEntity() };

        var created = (await handler.Create(entities, new Object(), CancellationToken.None)).ToList();

        Assert.That(created.Select(entity => entity.Identifier), Is.EqualTo(new[] { explicitIdentifier, factory }));
    }

    private class CountingCreationHandler() : CreationHandlerBase<Int32Entity, Int32, Object>
    {
        private Int32 _concurrency;

        public Int32 Calls { get; private set; }

        public Int32 MaximumConcurrency { get; private set; }

        protected override Int32 DoCreate(Int32Entity entity, Object? scope)
        {
            Calls++;

            return Calls;
        }

        protected override async Task<Int32> DoCreate(Int32Entity entity, Object? scope, CancellationToken cancellationToken)
        {
            _concurrency++;
            MaximumConcurrency = Math.Max(MaximumConcurrency, _concurrency);
            Calls++;

            var identifier = Calls;

            await Task.Delay(10, cancellationToken);

            _concurrency--;

            return identifier;
        }
    }

    private class EchoCreationHandler<TDomainEntity, TIdentifier>(CreationOptions<TIdentifier> options) : CreationHandlerBase<TDomainEntity, TIdentifier, Object>(options) where TDomainEntity : DomainEntityBase<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>
    {
        protected override TIdentifier? DoCreate(TDomainEntity entity, Object? scope)
        {
            return entity.Identifier;
        }

        protected override Task<TIdentifier?> DoCreate(TDomainEntity entity, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(entity.Identifier);
        }
    }

    private class StubCreationHandler<TDomainEntity, TIdentifier>(TIdentifier? identifier, CreationOptions<TIdentifier>? options = null) : CreationHandlerBase<TDomainEntity, TIdentifier, Object>(options) where TDomainEntity : DomainEntityBase<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier>
    {
        protected override TIdentifier? DoCreate(TDomainEntity entity, Object? scope)
        {
            return identifier;
        }

        protected override Task<TIdentifier?> DoCreate(TDomainEntity entity, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(identifier);
        }
    }

    public record GuidEntity : DomainEntityBase<Guid>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }
    }

    public record Int32Entity : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }
    }

    public record StringEntity : DomainEntityBase<String>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }
    }
}
