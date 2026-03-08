using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YuckQi.Application.Core.Abstract.Interfaces;
using YuckQi.Application.Core.Behaviors;
using YuckQi.Domain.Validation;

namespace YuckQi.Application.Core.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    [Test]
    public async Task Handle_WhenNoValidators_CallsNextAndReturnsResponse()
    {
        var validators = Array.Empty<AbstractValidator<PingRequest>>();
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest();
        var expected = new ValidatedStub();

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        Task<ValidatedStub> Next() => Task.FromResult(expected);
    }

    [Test]
    public async Task Handle_WhenValidatorsPass_CallsNextAndReturnsResponse()
    {
        var validators = new AbstractValidator<PingRequest>[] { new PassingValidator() };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "ok" };
        var expected = new ValidatedStub();

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        Task<ValidatedStub> Next() => Task.FromResult(expected);
    }

    [Test]
    public async Task Handle_WhenValidatorsFailWithError_ReturnsResponseWithValidationResultsAndDoesNotCallNext()
    {
        var validators = new AbstractValidator<PingRequest>[] { new FailingValidator() };
        var logger = new Mock<ILogger<ValidationBehavior<PingRequest, ValidatedStub>>>();
        var behavior = new ValidationBehavior<PingRequest, ValidatedStub>(validators, logger.Object);
        var request = new PingRequest { Value = "x" };
        var next = false;

        var result = await behavior.Handle(request, t => Next(), CancellationToken.None);

        Assert.That(next, Is.False);
        Assert.That(result.ValidationResults, Is.Not.Empty);

        Task<ValidatedStub> Next()
        {
            next = true;

            return Task.FromResult(new ValidatedStub());
        }
    }

    public sealed class PingRequest : IRequest<ValidatedStub>
    {
        public String Value { get; set; } = String.Empty;
    }

    public sealed class ValidatedStub : IValidated
    {
        public IReadOnlyCollection<Result> ValidationResults { get; set; } = Array.Empty<Result>();
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
}
