using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Aspects.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors.Validation;
using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.UnitTests.Behaviors.Validation;

public class ValidationBehaviorTests
{
    [Test]
    public async Task Handle_WhenNoValidators_CallsNextAndReturnsResponse()
    {
        var validators = Array.Empty<IValidator<PingRequest>>();
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest();
        var expected = new ValidatedStub();

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        ValueTask<ValidatedStub> Next() => new(expected);
    }

    [Test]
    public async Task Handle_WhenValidatorsPass_CallsNextAndReturnsResponse()
    {
        var validators = new IValidator<PingRequest>[] { new PassingValidator() };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "ok" };
        var expected = new ValidatedStub();

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        Assert.That(result.ValidationResults, Is.Empty);

        ValueTask<ValidatedStub> Next() => new(expected);
    }

    [Test]
    public async Task Handle_WhenValidatorsFailWithError_ReturnsResponseWithValidationResultsAndDoesNotCallNext()
    {
        var validators = new IValidator<PingRequest>[] { new FailingValidator() };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "x" };
        var next = false;

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(next, Is.False);
        Assert.That(result.ValidationResults, Is.Not.Empty);

        ValueTask<ValidatedStub> Next()
        {
            next = true;

            return new ValueTask<ValidatedStub>(new ValidatedStub());
        }
    }

    [Test]
    public async Task Handle_WhenSomeValidatorsFail_ReturnsOnlyResultsWithDetail()
    {
        var validators = new IValidator<PingRequest>[] { new NotEmptyValidator(), new FailingValidator() };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "x" };

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result.ValidationResults, Has.Count.EqualTo(1));
        Assert.That(result.ValidationResults.Single().Detail.Single().Message.ToString(), Is.EqualTo("too short"));

        ValueTask<ValidatedStub> Next() => new(new ValidatedStub());
    }

    [Test]
    public async Task Handle_WhenValidatorsShareAResource_RunsThemOneAtATime()
    {
        var resource = new ExclusiveResource();
        var validators = new IValidator<PingRequest>[] { new SharedResourceValidator(resource), new SharedResourceValidator(resource), new SharedResourceValidator(resource) };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "ok" };
        var expected = new ValidatedStub();

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        Assert.That(resource.Uses, Is.EqualTo(3));

        ValueTask<ValidatedStub> Next() => new(expected);
    }

    [Test]
    public async Task Handle_WhenValidatorsRegisteredFromAssembly_ResolvesAndRunsThem()
    {
        var services = new ServiceCollection();

        services.AddValidatorsFromAssemblyContaining<ValidationBehaviorTests>(ServiceLifetime.Scoped);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped(typeof(ValidationBehavior<,>));

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        var behavior = scope.ServiceProvider.GetRequiredService<ValidationBehavior<RegisteredRequest, ValidatedStub>>();
        var request = new RegisteredRequest();
        var next = false;

        var result = await behavior.Handle(request, (t, u) => Next(), CancellationToken.None);

        Assert.That(next, Is.False);
        Assert.That(result.ValidationResults.SelectMany(t => t.Detail).Select(t => t.Property), Is.EquivalentTo(new[] { nameof(RegisteredRequest.Name) }));

        ValueTask<ValidatedStub> Next()
        {
            next = true;

            return new ValueTask<ValidatedStub>(new ValidatedStub());
        }
    }

    public sealed class PingRequest : IRequest<ValidatedStub>
    {
        public String Value { get; set; } = String.Empty;
    }

    public sealed class RegisteredRequest : IRequest<ValidatedStub>
    {
        public String Name { get; set; } = String.Empty;
    }

    public sealed class ValidatedStub : IHasValidationResults
    {
        public IReadOnlyCollection<Result> ValidationResults { get; set; } = [];
    }

    public sealed class PassingValidator : AbstractValidator<PingRequest>
    {
        public PassingValidator()
        {
            RuleFor(t => t.Value).MinimumLength(2);
        }
    }

    public sealed class FailingValidator : AbstractValidator<PingRequest>
    {
        public FailingValidator()
        {
            RuleFor(t => t.Value).MinimumLength(2).WithMessage("too short");
        }
    }

    public sealed class NotEmptyValidator : AbstractValidator<PingRequest>
    {
        public NotEmptyValidator()
        {
            RuleFor(t => t.Value).NotEmpty();
        }
    }

    public sealed class RegisteredRequestValidator : AbstractValidator<RegisteredRequest>
    {
        public RegisteredRequestValidator()
        {
            RuleFor(t => t.Name).NotEmpty();
        }
    }

    public sealed class SharedResourceValidator : AbstractValidator<PingRequest>
    {
        public SharedResourceValidator(ExclusiveResource resource)
        {
            RuleFor(t => t.Value).MustAsync((t, u) => resource.IsAvailable(u));
        }
    }

    public sealed class ExclusiveResource
    {
        private Int32 _active;
        private Int32 _uses;

        public Int32 Uses => _uses;

        public async Task<Boolean> IsAvailable(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _active) > 1)
                throw new InvalidOperationException("A second operation was started on this resource before a previous operation completed.");

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken);

                Interlocked.Increment(ref _uses);

                return true;
            }
            finally
            {
                Interlocked.Decrement(ref _active);
            }
        }
    }
}
