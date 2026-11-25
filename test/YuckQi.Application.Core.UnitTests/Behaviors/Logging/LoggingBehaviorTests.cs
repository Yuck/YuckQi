using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Behaviors.Logging;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Logging;

public class LoggingBehaviorTests
{
    [Test]
    public async Task Handle_WhenNextReturns_InvokesNextAndReturnsResponse()
    {
        var expected = 42;
        var logger = new Mock<ILogger<LoggingBehavior<Ping, Int32>>>();
        var behavior = new LoggingBehavior<Ping, Int32>(logger.Object);
        var request = new Ping();

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));

        ValueTask<Int32> Next() => new(expected);
    }

    [Test]
    public async Task Handle_WhenNextReturns_LogsStartedAndCompleted()
    {
        var logger = new Mock<ILogger<LoggingBehavior<Ping, Int32>>>();
        var behavior = new LoggingBehavior<Ping, Int32>(logger.Object);
        var request = new Ping();

        await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        VerifyLog(logger, LogLevel.Information, "started", null, Times.Once());
        VerifyLog(logger, LogLevel.Information, "completed", null, Times.Once());

        ValueTask<Int32> Next() => new(0);
    }

    [Test]
    public async Task Handle_WhenNextThrows_LogsFailedWithElapsedAndRethrows()
    {
        var expected = new InvalidOperationException("boom");
        var logger = new Mock<ILogger<LoggingBehavior<Ping, Int32>>>();
        var behavior = new LoggingBehavior<Ping, Int32>(logger.Object);
        var request = new Ping();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Handle(request, (t, u) => Next(), CancellationToken.None));

        Assert.That(actual, Is.SameAs(expected));
        VerifyLog(logger, LogLevel.Error, "failed", expected, Times.Once());
        VerifyLog(logger, LogLevel.Error, "elapsed", expected, Times.Once());
        VerifyLog(logger, LogLevel.Information, "completed", null, Times.Never());

        ValueTask<Int32> Next() => throw expected;
    }

    [Test]
    public async Task Handle_WhenCanceled_LogsCanceledAndRethrows()
    {
        using var source = new CancellationTokenSource();
        var logger = new Mock<ILogger<LoggingBehavior<Ping, Int32>>>();
        var behavior = new LoggingBehavior<Ping, Int32>(logger.Object);
        var request = new Ping();

        source.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await behavior.Handle(request, (t, u) => Next(u), source.Token));

        VerifyLog(logger, LogLevel.Information, "canceled", null, Times.Once());
        VerifyLog(logger, LogLevel.Error, "failed", null, Times.Never());

        ValueTask<Int32> Next(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<Int32>(0);
        }
    }

    private static void VerifyLog(Mock<ILogger<LoggingBehavior<Ping, Int32>>> logger, LogLevel level, String text, Exception? exception, Times times)
    {
        logger.Verify(t => t.Log(level, It.IsAny<EventId>(), It.Is<It.IsAnyType>((u, v) => IsLogStateContaining(u, text)), It.Is<Exception?>(u => exception == null || u == exception), It.IsAny<Func<It.IsAnyType, Exception?, String>>()), times);
    }

    private static Boolean IsLogStateContaining(Object? state, String text)
    {
        return state?.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) is true;
    }

    public sealed class Ping : IRequest<Int32>;
}
