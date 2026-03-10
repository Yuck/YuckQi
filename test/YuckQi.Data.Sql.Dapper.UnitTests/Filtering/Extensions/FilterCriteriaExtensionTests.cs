using System.Data;
using NUnit.Framework;
using YuckQi.Data.Filtering;
using YuckQi.Data.Sql.Dapper.Filtering.Extensions;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Filtering.Extensions;

public class FilterCriteriaExtensionTests
{
    [Test]
    public void FilterCriteria_SingleValue_IsValid()
    {
        var criteria = new[] { new FilterCriteria("thing", "a test") };
        var parameters = criteria.ToDynamicParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(1));
            Assert.That(parameters.ParameterNames.First(), Is.EqualTo("thing"));
            Assert.That(parameters.Get<Object>("thing"), Is.EqualTo("a test"));
        });
    }

    [Test]
    public void FilterCriteria_SingleNullValue_IsValid()
    {
        var criteria = new[] { new FilterCriteria("thing", null) };
        var parameters = criteria.ToDynamicParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(1));
            Assert.That(parameters.ParameterNames.First(), Is.EqualTo("thing"));
            Assert.That(parameters.Get<Object>("thing"), Is.Null);
        });
    }

    [Test]
    public void FilterCriteria_EmptyList_IsValid()
    {
        var parameters = new List<FilterCriteria>().ToDynamicParameters();

        Assert.That(parameters.ParameterNames.Count(), Is.Zero);
    }

    [Test]
    public void FilterCriteria_MultipleValues_IsValid()
    {
        var criteria = new[] { new FilterCriteria("thing", "a test"), new FilterCriteria("other", 1234.56M) };
        var parameters = criteria.ToDynamicParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(2));
            Assert.That(parameters.ParameterNames.First(), Is.EqualTo("thing"));
            Assert.That(parameters.Get<Object>("thing"), Is.EqualTo("a test"));
            Assert.That(parameters.ParameterNames.Last(), Is.EqualTo("other"));
            Assert.That(parameters.Get<Object>("other"), Is.EqualTo(1234.56M));
        });
    }

    [Test]
    public void FilterCriteria_NullParameters_ReturnsEmptyParameters()
    {
        var parameters = ((IEnumerable<FilterCriteria>?) null).ToDynamicParameters();

        Assert.That(parameters.ParameterNames.Count(), Is.Zero);
    }

    [Test]
    public void FilterCriteria_InOperation_IsValid()
    {
        var criteria = new[] { new FilterCriteria("thing", FilterOperation.In, new[] { 10, 20, 30 }) };
        var parameters = criteria.ToDynamicParameters();

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(3));
            Assert.That(parameters.Get<Object>("thing0"), Is.EqualTo(10));
            Assert.That(parameters.Get<Object>("thing1"), Is.EqualTo(20));
            Assert.That(parameters.Get<Object>("thing2"), Is.EqualTo(30));
        });
    }

    [Test]
    public void FilterCriteria_InOperation_WithNonEnumerableValue_ThrowsArgumentException()
    {
        var criteria = new[] { new FilterCriteria("thing", FilterOperation.In, 42) };

        Assert.Throws<ArgumentException>(() => criteria.ToDynamicParameters());
    }

    [Test]
    public void FilterCriteria_WithDbTypeMap_IsValid()
    {
        var dbTypeMap = new Dictionary<Type, DbType> { { typeof(String), DbType.AnsiString } };
        var criteria = new[] { new FilterCriteria("thing", "a test") };
        var parameters = criteria.ToDynamicParameters(dbTypeMap);

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(1));
            Assert.That(parameters.Get<Object>("thing"), Is.EqualTo("a test"));
        });
    }

    [Test]
    public void FilterCriteria_InOperation_WithDbTypeMap_IsValid()
    {
        var dbTypeMap = new Dictionary<Type, DbType> { { typeof(Int32), DbType.Int32 } };
        var criteria = new[] { new FilterCriteria("thing", FilterOperation.In, new[] { 10, 20 }) };
        var parameters = criteria.ToDynamicParameters(dbTypeMap);

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(2));
            Assert.That(parameters.Get<Object>("thing0"), Is.EqualTo(10));
            Assert.That(parameters.Get<Object>("thing1"), Is.EqualTo(20));
        });
    }

    [Test]
    public void FilterCriteria_WithNullValueAndDbTypeMap_IsValid()
    {
        var dbTypeMap = new Dictionary<Type, DbType> { { typeof(String), DbType.AnsiString } };
        var criteria = new[] { new FilterCriteria("thing", null) };
        var parameters = criteria.ToDynamicParameters(dbTypeMap);

        Assert.Multiple(() =>
        {
            Assert.That(parameters.ParameterNames.Count(), Is.EqualTo(1));
            Assert.That(parameters.Get<Object>("thing"), Is.Null);
        });
    }
}
