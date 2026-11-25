using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Handlers.Read.Abstract;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.UnitTests.Handlers.Read;

public class RetrievalHandlerBaseTests
{
    [Test]
    public void Get_ByObject_WithNullableIdentifier_GetsByIdentifier()
    {
        var handler = new StubRetrievalHandler();
        Int32? identifier = 5;

        var result = handler.Get(identifier, new Object());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result?.Identifier, Is.EqualTo(5));
            Assert.That(handler.RequestedIdentifier, Is.EqualTo(5));
            Assert.That(handler.RequestedParameters, Is.Null);
        }
    }

    [Test]
    public async Task Get_ByObject_Async_WithNullableIdentifier_GetsByIdentifier()
    {
        var handler = new StubRetrievalHandler();
        Int32? identifier = 5;

        var result = await handler.Get(identifier, new Object(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result?.Identifier, Is.EqualTo(5));
            Assert.That(handler.RequestedIdentifier, Is.EqualTo(5));
            Assert.That(handler.RequestedParameters, Is.Null);
        }
    }

    [Test]
    public void Get_ByObject_WithMistypedIdentifier_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.Get(5L, new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public async Task Get_ByObject_Async_WithMistypedIdentifier_Throws()
    {
        var handler = new StubRetrievalHandler();

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Get(5L, new Object(), CancellationToken.None));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void Get_ByObject_WithProperties_GetsByFilters()
    {
        var handler = new StubRetrievalHandler();

        handler.Get(new { Name = "ABC" }, new Object());

        Assert.That(handler.RequestedParameters, Is.EqualTo(new[] { new FilterCriteria("Name", "ABC") }));
    }

    [Test]
    public void Get_ByFilterCriteria_WhenEmpty_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.Get(Array.Empty<FilterCriteria>(), new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public async Task Get_ByFilterCriteria_Async_WhenEmpty_Throws()
    {
        var handler = new StubRetrievalHandler();

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Get(Array.Empty<FilterCriteria>(), new Object(), CancellationToken.None));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void Get_ByFilterCriteria_WithUnknownOperation_Throws()
    {
        var handler = new StubRetrievalHandler();
        var parameters = new[] { new FilterCriteria("TenantId", FilterOperation.Unknown, 7) };

        Assert.Throws<ArgumentException>(() => handler.Get(parameters, new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void GetList_ByObject_WithMistypedIdentifier_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.GetList(5L, new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void GetList_ByObject_WithDictionary_GetsByFilters()
    {
        var handler = new StubRetrievalHandler();
        var parameters = new Dictionary<String, Object?> { ["TenantId"] = 7 };

        handler.GetList(parameters, new Object());

        Assert.That(handler.RequestedParameters, Is.EqualTo(new[] { new FilterCriteria("TenantId", 7) }));
    }

    [Test]
    public async Task GetList_ByFilterCriteria_WithUnknownOperation_Throws()
    {
        var handler = new StubRetrievalHandler();
        var parameters = new[] { new FilterCriteria("TenantId", FilterOperation.Unknown, 7) };

        await Assert.ThrowsAsync<ArgumentException>(() => handler.GetList(parameters, new Object(), CancellationToken.None));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void GetList_ByFilterCriteria_WhenEmpty_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.GetList(Array.Empty<FilterCriteria>(), new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void GetList_ByObject_WithEmptyDictionary_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.GetList(new Dictionary<String, Object?>(), new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void Get_ByFilterCriteria_WithNullFilter_Throws()
    {
        var handler = new StubRetrievalHandler();

        Assert.Throws<ArgumentException>(() => handler.Get(new FilterCriteria[1], new Object()));
        Assert.That(handler.RequestedParameters, Is.Null);
    }

    [Test]
    public void GetList_WithOnlyScope_GetsUnfilteredList()
    {
        var handler = new StubRetrievalHandler();

        handler.GetList(new Object());

        Assert.That(handler.RequestedParameters, Is.Null);
    }

    public record SurLaTable : DomainEntityBase<Int32>;

    private class StubRetrievalHandler : RetrievalHandlerBase<SurLaTable, Int32, Object>
    {
        public Int32? RequestedIdentifier { get; private set; }

        public IReadOnlyCollection<FilterCriteria>? RequestedParameters { get; private set; }

        protected override SurLaTable DoGet(Int32 identifier, Object? scope)
        {
            RequestedIdentifier = identifier;

            return new SurLaTable { Identifier = identifier };
        }

        protected override Task<SurLaTable?> DoGet(Int32 identifier, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult<SurLaTable?>(DoGet(identifier, scope));
        }

        protected override SurLaTable? DoGet(IReadOnlyCollection<FilterCriteria> parameters, Object? scope)
        {
            RequestedParameters = parameters;

            return null;
        }

        protected override Task<SurLaTable?> DoGet(IReadOnlyCollection<FilterCriteria> parameters, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(DoGet(parameters, scope));
        }

        protected override IReadOnlyCollection<SurLaTable> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, Object? scope)
        {
            RequestedParameters = parameters;

            return [];
        }

        protected override Task<IReadOnlyCollection<SurLaTable>> DoGetList(IReadOnlyCollection<FilterCriteria>? parameters, Object? scope, CancellationToken cancellationToken)
        {
            return Task.FromResult(DoGetList(parameters, scope));
        }
    }
}
