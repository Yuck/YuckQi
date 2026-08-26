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

        logger.Verify(t => t.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((t, u) => t.ToString()!.Contains("started", StringComparison.OrdinalIgnoreCase)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, String>>()), Times.Once);
        logger.Verify(t => t.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((t, u) => t.ToString()!.Contains("completed", StringComparison.OrdinalIgnoreCase)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, String>>()), Times.Once);

        ValueTask<Int32> Next() => new(0);
    }

    public sealed class Ping : IRequest<Int32>;
}
