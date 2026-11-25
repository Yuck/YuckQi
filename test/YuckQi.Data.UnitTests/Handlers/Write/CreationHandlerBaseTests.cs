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
