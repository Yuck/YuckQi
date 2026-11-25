using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using NUnit.Framework;
using YuckQi.Data.Sql.Dapper.Handlers.Write;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract;

namespace YuckQi.Data.Sql.Dapper.UnitTests.Handlers.Write;

public class TemporalScaffoldTests
{
    [OneTimeSetUp]
    public void RegisterDateOnlyAndTimeOnlyTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }

    [Test]
    public void Create_WithTemporalProperties_InsertsAllColumns()
    {
        var commands = new List<String>();
        var handler = new CreationHandler<ScheduleRecord, Int32, IDbTransaction>();

        handler.Create(CreateSchedule(), MockDbFactory.CreateTransaction(commands));

        AssertInsertsTemporalColumns(commands.Single());
    }

    [Test]
    public async Task CreateAsync_WithTemporalProperties_InsertsAllColumns()
    {
        var commands = new List<String>();
        var handler = new CreationHandler<ScheduleRecord, Int32, IDbTransaction>();

        await handler.Create(CreateSchedule(), MockDbFactory.CreateTransaction(commands), CancellationToken.None);

        AssertInsertsTemporalColumns(commands.Single());
    }

    [Test]
    public void Revise_WithTemporalProperties_UpdatesAllColumns()
    {
        var commands = new List<String>();
        var handler = new RevisionHandler<ScheduleRecord, Int32, IDbTransaction>(null);

        handler.Revise(CreateSchedule(), MockDbFactory.CreateTransaction(commands));

        AssertUpdatesTemporalColumns(commands.Single());
    }

    [Test]
    public async Task ReviseAsync_WithTemporalProperties_UpdatesAllColumns()
    {
        var commands = new List<String>();
        var handler = new RevisionHandler<ScheduleRecord, Int32, IDbTransaction>(null);

        await handler.Revise(CreateSchedule(), MockDbFactory.CreateTransaction(commands), CancellationToken.None);

        AssertUpdatesTemporalColumns(commands.Single());
    }

    [Test]
    public void Create_CalledRepeatedly_DoesNotRepeatColumns()
    {
        var commands = new List<String>();
        var handler = new CreationHandler<ScheduleRecord, Int32, IDbTransaction>();

        handler.Create(CreateSchedule(), MockDbFactory.CreateTransaction(commands));
        handler.Create(CreateSchedule(), MockDbFactory.CreateTransaction(commands));

        Assert.That(commands, Has.Count.EqualTo(2));
        Assert.That(commands[1], Is.EqualTo(commands[0]));
        Assert.That(CountOccurrences(commands[0], "[StartLocalTime]"), Is.EqualTo(1));
    }

    [Test]
    public void Create_AfterDirectSimpleCrudInsert_DoesNotRepeatColumns()
    {
        var commands = new List<String>();
        var handler = new CreationHandler<WarmCacheRecord, Int32, IDbTransaction>();
        var record = new WarmCacheRecord { Identifier = 1, Day = new DateOnly(2026, 11, 25) };

        MockDbFactory.CreateConnection(commands).Insert<Int32?, WarmCacheRecord>(record);
        handler.Create(record, MockDbFactory.CreateTransaction(commands));
        handler.Create(record, MockDbFactory.CreateTransaction(commands));

        Assert.That(commands[0], Does.Not.Contain("[Day]"));
        Assert.That(CountOccurrences(commands[1], "[Day]"), Is.EqualTo(1));
        Assert.That(commands[2], Is.EqualTo(commands[1]));
    }

    [Test]
    public void Create_WithAttributes_MatchesSimpleCrudDateTimeBehavior()
    {
        var temporal = new List<String>();
        var baseline = new List<String>();

        new CreationHandler<TemporalParityRecord, Int32, IDbTransaction>().Create(new TemporalParityRecord { Identifier = 1 }, MockDbFactory.CreateTransaction(temporal));
        new CreationHandler<DateTimeParityRecord, Int32, IDbTransaction>().Create(new DateTimeParityRecord { Identifier = 1 }, MockDbFactory.CreateTransaction(baseline));

        Assert.That(DescribeInsertIgnoringColumnOrder(temporal.Single()), Is.EqualTo(DescribeInsertIgnoringColumnOrder(baseline.Single())));
    }

    [Test]
    public void Revise_WithAttributes_MatchesSimpleCrudDateTimeBehavior()
    {
        var temporal = new List<String>();
        var baseline = new List<String>();

        new RevisionHandler<TemporalParityRecord, Int32, IDbTransaction>(null).Revise(new TemporalParityRecord { Identifier = 1 }, MockDbFactory.CreateTransaction(temporal));
        new RevisionHandler<DateTimeParityRecord, Int32, IDbTransaction>(null).Revise(new DateTimeParityRecord { Identifier = 1 }, MockDbFactory.CreateTransaction(baseline));

        Assert.That(DescribeUpdateIgnoringColumnOrder(temporal.Single()), Is.EqualTo(DescribeUpdateIgnoringColumnOrder(baseline.Single())));
    }

    private static String DescribeInsertIgnoringColumnOrder(String sql)
    {
        var match = Regex.Match(sql, @"^(insert into \S+) \((.*)\) values \((.*)\)$");

        Assert.That(match.Success, Is.True, sql);

        var columns = match.Groups[2].Value.Split(", ");
        var values = match.Groups[3].Value.Split(", ");

        Assert.That(values, Has.Length.EqualTo(columns.Length), sql);

        return $"{match.Groups[1].Value}: {String.Join(", ", columns.Zip(values, (column, value) => $"{column}={value}").Order())}";
    }

    private static String DescribeUpdateIgnoringColumnOrder(String sql)
    {
        var match = Regex.Match(sql, @"^(update \S+) set (.*) (where .*)$");

        Assert.That(match.Success, Is.True, sql);

        return $"{match.Groups[1].Value}: {String.Join(", ", match.Groups[2].Value.Split(", ").Order())} {match.Groups[3].Value}";
    }

    private static void AssertInsertsTemporalColumns(String sql)
    {
        Assert.That(sql, Does.StartWith("insert into [Schedule]"));
        Assert.That(sql, Does.Contain("[EffectiveDate]").And.Contain("@EffectiveDate"));
        Assert.That(sql, Does.Contain("[ExpirationDate]").And.Contain("@ExpirationDate"));
        Assert.That(sql, Does.Contain("[StartLocalTime]").And.Contain("@StartLocalTime"));
        Assert.That(sql, Does.Contain("[EndLocalTime]").And.Contain("@EndLocalTime"));
        Assert.That(sql, Does.Contain("[DaysOfWeekMask]").And.Contain("@DaysOfWeekMask"));
    }

    private static void AssertUpdatesTemporalColumns(String sql)
    {
        Assert.That(sql, Does.StartWith("update [Schedule]"));
        Assert.That(sql, Does.Contain("[EffectiveDate] = @EffectiveDate"));
        Assert.That(sql, Does.Contain("[ExpirationDate] = @ExpirationDate"));
        Assert.That(sql, Does.Contain("[StartLocalTime] = @StartLocalTime"));
        Assert.That(sql, Does.Contain("[EndLocalTime] = @EndLocalTime"));
        Assert.That(sql, Does.Contain("[DaysOfWeekMask] = @DaysOfWeekMask"));
    }

    private static Int32 CountOccurrences(String text, String value)
    {
        return text.Split(value).Length - 1;
    }

    private static ScheduleRecord CreateSchedule()
    {
        return new ScheduleRecord
        {
            Identifier = 1,
            DaysOfWeekMask = 0b0011111,
            EffectiveDate = new DateOnly(2026, 11, 25),
            ExpirationDate = new DateOnly(2027, 11, 25),
            StartLocalTime = new TimeOnly(9, 0),
            EndLocalTime = new TimeOnly(17, 30)
        };
    }

    [Table("Schedule")]
    public record ScheduleRecord : DomainEntityBase<Int32>, ICreationMoment, IRevisionMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public Int32 DaysOfWeekMask { get; set; }

        public DateOnly EffectiveDate { get; set; }

        public TimeOnly? EndLocalTime { get; set; }

        public DateOnly? ExpirationDate { get; set; }

        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        public DateTimeOffset RevisionMoment { get; set; }

        public TimeOnly StartLocalTime { get; set; }
    }

    [Table("WarmCache")]
    public record WarmCacheRecord : DomainEntityBase<Int32>, ICreationMoment
    {
        public DateTimeOffset CreationMoment { get; set; }

        public DateOnly Day { get; set; }

        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }
    }

    [Table("Parity")]
    public record TemporalParityRecord : DomainEntityBase<Int32>, ICreationMoment, IRevisionMoment
    {
        [Column("custom_column")]
        public DateOnly Custom { get; set; }

        public DateTimeOffset CreationMoment { get; set; }

        [Editable(true)]
        public DateOnly Editable { get; set; }

        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        [IgnoreInsert]
        public TimeOnly InsertIgnored { get; set; }

        [Editable(false)]
        public DateOnly NotEditable { get; set; }

        [NotMapped]
        public TimeOnly? NotMapped { get; set; }

        public DateOnly Plain { get; set; }

        [ReadOnly(true)]
        public TimeOnly ReadOnly { get; set; }

        public DateTimeOffset RevisionMoment { get; set; }

        [IgnoreUpdate]
        public DateOnly? UpdateIgnored { get; set; }

        [ReadOnly(false)]
        public TimeOnly Writable { get; set; }
    }

    [Table("Parity")]
    public record DateTimeParityRecord : DomainEntityBase<Int32>, ICreationMoment, IRevisionMoment
    {
        [Column("custom_column")]
        public DateTime Custom { get; set; }

        public DateTimeOffset CreationMoment { get; set; }

        [Editable(true)]
        public DateTime Editable { get; set; }

        [Key]
        public Int32 Id { get => Identifier; set => Identifier = value; }

        [IgnoreInsert]
        public DateTime InsertIgnored { get; set; }

        [Editable(false)]
        public DateTime NotEditable { get; set; }

        [NotMapped]
        public DateTime? NotMapped { get; set; }

        public DateTime Plain { get; set; }

        [ReadOnly(true)]
        public DateTime ReadOnly { get; set; }

        public DateTimeOffset RevisionMoment { get; set; }

        [IgnoreUpdate]
        public DateTime? UpdateIgnored { get; set; }

        [ReadOnly(false)]
        public DateTime Writable { get; set; }
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(Object value)
        {
            return DateOnly.FromDateTime((DateTime) value);
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }
    }

    private sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(Object value)
        {
            return TimeOnly.FromTimeSpan((TimeSpan) value);
        }

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value.ToTimeSpan();
        }
    }
}
