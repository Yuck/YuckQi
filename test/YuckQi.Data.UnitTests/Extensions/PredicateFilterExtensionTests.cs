using System.Linq.Expressions;
using NUnit.Framework;
using YuckQi.Data.Extensions;
using YuckQi.Data.Filtering;
using YuckQi.Data.Filtering.Extensions;

namespace YuckQi.Data.UnitTests.Extensions;

public class PredicateFilterExtensionTests
{
    [Test]
    public void Predicate_WithSingleEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test";

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filters.First().Value, Is.EqualTo("test"));
        });
    }

    [Test]
    public void Predicate_WithAndAlso_IsConvertedToMultipleFilterExpressions()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test" && t.Identifier > 10;

        var filters = predicate.ToFilterExpressions().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(2));
            Assert.That(filters[0].Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filters[0].Value, Is.EqualTo("test"));
            Assert.That(filters[1].Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(filters[1].Value, Is.EqualTo(10));
        });
    }

    [Test]
    public void Predicate_WithNotEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name != "exclude";

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.NotEqual));
            Assert.That(filters.First().Value, Is.EqualTo("exclude"));
        });
    }

    [Test]
    public void Predicate_WithLessThan_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier < 100;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.LessThan));
            Assert.That(filters.First().Value, Is.EqualTo(100));
        });
    }

    [Test]
    public void Predicate_WithLessThanOrEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier <= 50;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.LessThanOrEqual));
            Assert.That(filters.First().Value, Is.EqualTo(50));
        });
    }

    [Test]
    public void Predicate_WithGreaterThanOrEqual_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Identifier >= 1;

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.GreaterThanOrEqual));
            Assert.That(filters.First().Value, Is.EqualTo(1));
        });
    }

    [Test]
    public void Predicate_WithStringContains_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Contains("test");

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.Contains));
            Assert.That(filters.First().Value, Is.EqualTo("test"));
        });
    }

    [Test]
    public void Predicate_WithCapturedStringContains_IsConvertedToFilterExpression()
    {
        var query = "Guy";
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Contains(query);

        var filters = predicate.ToFilterExpressions();

        Assert.Multiple(() =>
        {
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.First().Operation, Is.EqualTo(FilterOperation.Contains));
            Assert.That(filters.First().Value, Is.EqualTo("Guy"));
        });
    }

    [Test]
    public void Predicate_WithStringContains_AndNullValue_ThrowsArgumentException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Contains(null!);

        Assert.That(() => predicate.ToFilterExpressions(), Throws.ArgumentException);
    }

    [Test]
    public void Predicate_WithUnsupportedBinaryExpression_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "a" || t.Name == "b";

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_ToFilterExpressionsThenToFilterCriteria_ProducesMatchingFilterCriteria()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == "test" && t.Identifier > 10;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.Count, Is.EqualTo(2));
            Assert.That(criteria[0].FieldName, Is.EqualTo("Name"));
            Assert.That(criteria[0].Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria[0].Value, Is.EqualTo("test"));
            Assert.That(criteria[1].FieldName, Is.EqualTo("Identifier"));
            Assert.That(criteria[1].Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria[1].Value, Is.EqualTo(10));
        });
    }

    [TestCase(FilterOperation.Equal)]
    [TestCase(FilterOperation.NotEqual)]
    [TestCase(FilterOperation.GreaterThan)]
    [TestCase(FilterOperation.GreaterThanOrEqual)]
    [TestCase(FilterOperation.LessThan)]
    [TestCase(FilterOperation.LessThanOrEqual)]
    public void Predicate_WithConstantOnLeft_IsMirrored(FilterOperation operation)
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = operation switch
        {
            FilterOperation.Equal => t => 18 == t.Identifier,
            FilterOperation.NotEqual => t => 18 != t.Identifier,
            FilterOperation.GreaterThan => t => 18 > t.Identifier,
            FilterOperation.GreaterThanOrEqual => t => 18 >= t.Identifier,
            FilterOperation.LessThan => t => 18 < t.Identifier,
            _ => t => 18 <= t.Identifier
        };
        var expected = operation switch
        {
            FilterOperation.GreaterThan => FilterOperation.LessThan,
            FilterOperation.GreaterThanOrEqual => FilterOperation.LessThanOrEqual,
            FilterOperation.LessThan => FilterOperation.GreaterThan,
            FilterOperation.LessThanOrEqual => FilterOperation.GreaterThanOrEqual,
            _ => operation
        };

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Identifier"));
            Assert.That(criteria.Operation, Is.EqualTo(expected));
            Assert.That(criteria.Value, Is.EqualTo(18));
        });
    }

    [Test]
    public void Predicate_WithCapturedVariable_IsConvertedToFilterExpression()
    {
        var name = "Guy";
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == name;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filter.Value, Is.EqualTo("Guy"));
        });
    }

    [Test]
    public void Predicate_WithCapturedVariableOnLeft_IsMirrored()
    {
        var cutoff = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => cutoff < t.Moment;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Moment"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria.Value, Is.EqualTo(cutoff));
        });
    }

    [Test]
    public void Predicate_WithNestedCapturedMember_IsConvertedToFilterExpression()
    {
        var request = new TestRequest { Name = "Guy", Minimum = 5 };
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == request.Name && t.Identifier >= request.Minimum;

        var filters = predicate.ToFilterExpressions().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(filters[0].Value, Is.EqualTo("Guy"));
            Assert.That(filters[1].Operation, Is.EqualTo(FilterOperation.GreaterThanOrEqual));
            Assert.That(filters[1].Value, Is.EqualTo(5));
        });
    }

    [Test]
    public void Predicate_WithStaticMember_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Moment < DateTimeOffset.MaxValue;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.That(filter.Value, Is.EqualTo(DateTimeOffset.MaxValue));
    }

    [Test]
    public void Predicate_WithComputedValue_IsEvaluated()
    {
        var cutoff = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Moment > cutoff.AddDays(-1);

        var filter = predicate.ToFilterExpressions().Single();

        Assert.That(filter.Value, Is.EqualTo(cutoff.AddDays(-1)));
    }

    [Test]
    public void Predicate_WithNullableMemberAndLiteral_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Age == 5;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Age"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(criteria.Value, Is.EqualTo(5));
        });
    }

    [Test]
    public void Predicate_WithNullableMemberAndCapturedValue_IsConvertedToFilterExpression()
    {
        var minimum = 21;
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => minimum <= t.Age;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.GreaterThanOrEqual));
            Assert.That(filter.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Predicate_WithNullableMemberAndNull_IsConvertedToFilterExpression()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Age == null;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.Equal));
            Assert.That(filter.Value, Is.Null);
        });
    }

    [Test]
    public void Predicate_WithValueOfNullableMember_UsesNullableMemberPath()
    {
        var parameter = Expression.Parameter(typeof(TestDomainEntity), "t");
        var member = Expression.Property(Expression.Property(parameter, nameof(TestDomainEntity.Age)), nameof(Nullable<Int32>.Value));
        var predicate = Expression.Lambda<Func<TestDomainEntity, Boolean>>(Expression.GreaterThan(member, Expression.Constant(18)), parameter);

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Age"));
            Assert.That(criteria.Operation, Is.EqualTo(FilterOperation.GreaterThan));
            Assert.That(criteria.Value, Is.EqualTo(18));
        });
    }

    [Test]
    public void Predicate_WithHasValueOfNullableMember_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Age.HasValue == true;

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_WithEnumMember_KeepsEnumValue()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Status == TestStatus.Active;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Status"));
            Assert.That(criteria.Value, Is.EqualTo(TestStatus.Active));
        });
    }

    [Test]
    public void Predicate_WithNullableEnumMemberAndCapturedValue_KeepsEnumValue()
    {
        var status = TestStatus.Active;
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.PreviousStatus != status;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.NotEqual));
            Assert.That(filter.Value, Is.EqualTo(TestStatus.Active));
        });
    }

    [Test]
    public void Predicate_WithByteEnumMember_KeepsEnumValue()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Level == TestLevel.High;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.That(filter.Value, Is.EqualTo(TestLevel.High));
    }

    [Test]
    public void Predicate_WithInt16EnumMemberAndCapturedValue_KeepsEnumValue()
    {
        var priority = TestPriority.Urgent;
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => priority <= t.Priority;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.GreaterThanOrEqual));
            Assert.That(filter.Value, Is.EqualTo(TestPriority.Urgent));
        });
    }

    [Test]
    public void Predicate_WithByteEnumMemberAndOutOfRangeValue_KeepsIntegerValue()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => (Int32) t.Level == 300;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.That(filter.Value, Is.EqualTo(300));
    }

    [Test]
    public void Predicate_WithHasValueOfNullCapturedValue_IsFalse()
    {
        Int32? age = null;
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.IsActive == age.HasValue;

        var filter = predicate.ToFilterExpressions().Single();

        Assert.That(filter.Value, Is.False);
    }

    [Test]
    public void Predicate_WithValueOfNullValue_ThrowsInvalidOperationException()
    {
        var value = Expression.Property(Expression.Constant(null, typeof(Int32?)), nameof(Nullable<Int32>.Value));
        var predicate = ToEqualPredicate(nameof(TestDomainEntity.Identifier), value);

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InvalidOperationException);
    }

    [Test]
    public void Predicate_WithMemberOfNullObject_ThrowsNullReferenceException()
    {
        var value = Expression.Property(Expression.Constant(null, typeof(TestRequest)), nameof(TestRequest.Name));
        var predicate = ToEqualPredicate(nameof(TestDomainEntity.Name), value);

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NullReferenceException>());
    }

    [Test]
    public void Predicate_WithNestedMember_UsesFullPath()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => "Paris" == t.Address.City;

        var criteria = predicate.ToFilterExpressions().ToFilterCriteria<TestDomainEntity, TestDomainEntity>(null).Single();

        Assert.Multiple(() =>
        {
            Assert.That(criteria.FieldName, Is.EqualTo("Address.City"));
            Assert.That(criteria.Value, Is.EqualTo("Paris"));
        });
    }

    [Test]
    public void Predicate_WithNestedCapturedStringContains_IsConvertedToFilterExpression()
    {
        var request = new TestRequest { Name = "Guy" };
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Contains(request.Name);

        var filter = predicate.ToFilterExpressions().Single();

        Assert.Multiple(() =>
        {
            Assert.That(filter.Operation, Is.EqualTo(FilterOperation.Contains));
            Assert.That(filter.Value, Is.EqualTo("Guy"));
        });
    }

    [Test]
    public void Predicate_WithMemberOnBothSides_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name == t.Address.City;

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_WithNoMember_ThrowsNotSupportedException()
    {
        var name = "Guy";
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => name == "Guy";

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_WithMethodCallOnMember_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.ToLower() == "guy";

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_WithStringContainsOnMemberArgument_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Contains(t.Address.City);

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Predicate_WithStringContainsOnComputedMember_ThrowsNotSupportedException()
    {
        Expression<Func<TestDomainEntity, Boolean>> predicate = t => t.Name.Trim().Contains("Guy");

        Assert.That(() => predicate.ToFilterExpressions(), Throws.InstanceOf<NotSupportedException>());
    }

    private static Expression<Func<TestDomainEntity, Boolean>> ToEqualPredicate(String memberName, Expression value)
    {
        var parameter = Expression.Parameter(typeof(TestDomainEntity), "t");
        var member = Expression.Property(parameter, memberName);

        return Expression.Lambda<Func<TestDomainEntity, Boolean>>(Expression.Equal(member, value), parameter);
    }

    private enum TestLevel : Byte
    {
        Low = 1,
        High = 2
    }

    private enum TestPriority : Int16
    {
        Normal = 1,
        Urgent = 2
    }

    private enum TestStatus
    {
        Inactive = 0,
        Active = 1
    }

    private sealed class TestAddress
    {
        public String City { get; set; } = String.Empty;
    }

    private sealed class TestDomainEntity
    {
        public TestAddress Address { get; set; } = new();

        public Int32? Age { get; set; }

        public Int32 Identifier { get; set; }

        public Boolean IsActive { get; set; }

        public TestLevel Level { get; set; }

        public DateTimeOffset Moment { get; set; }

        public String Name { get; set; } = String.Empty;

        public TestStatus? PreviousStatus { get; set; }

        public TestPriority Priority { get; set; }

        public TestStatus Status { get; set; }
    }

    private sealed class TestRequest
    {
        public Int32 Minimum { get; set; }

        public String Name { get; set; } = String.Empty;
    }
}
