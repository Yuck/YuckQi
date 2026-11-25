using FluentValidation;
using FluentValidation.Results;
using NUnit.Framework;
using YuckQi.Domain.Validation.Extensions;

namespace YuckQi.Domain.Validation.UnitTests.Extensions;

public class AbstractValidatorExtensionsTests
{
    public class StringValidator : AbstractValidator<String>
    {
        public StringValidator()
        {
            RuleFor(t => t.Length).GreaterThan(1).WithMessage("it is too short");
        }
    }

    public class Profile
    {
        public String Email { get; set; } = String.Empty;
        public String Nickname { get; set; } = String.Empty;
    }

    public class ProfileValidator : AbstractValidator<Profile>
    {
        public ProfileValidator()
        {
            RuleFor(t => t.Email).NotEmpty();
            RuleFor(t => t.Email).Must(t => t != "taken@example.com").WithErrorCode("DuplicateEmail");
            RuleFor(t => t.Nickname).MaximumLength(20).WithSeverity(Severity.Warning);
            RuleFor(t => t.Nickname).Must(t => t != "admin").WithSeverity(Severity.Info);
            RuleFor(t => t).Custom((t, u) =>
            {
                if (t.Nickname == "anonymous")
                    u.AddFailure(new ValidationFailure(nameof(Profile.Nickname), "it is anonymous"));
            });
        }
    }

    [Test]
    public void AbstractValidator_GetResultWithValidItem_IsValid()
    {
        var validator = new StringValidator();
        var result = validator.GetResult("hello");

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task AbstractValidator_GetResultAsyncWithValidItem_IsValid()
    {
        var validator = new StringValidator();
        var result = await validator.GetResult("hello", CancellationToken.None);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void AbstractValidator_GetResultWithInvalidItem_HasDetail()
    {
        var validator = new StringValidator();
        var result = validator.GetResult(String.Empty);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Detail, Is.Not.Empty);
    }

    [Test]
    public void AbstractValidator_GetResultWithValidItem_HasNoDetail()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "someone@example.com", Nickname = "someone" };
        var result = validator.GetResult(profile);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content, Is.SameAs(profile));
        Assert.That(result.Detail, Is.Empty);
    }

    [Test]
    public void AbstractValidator_GetResultWithWarningSeverityFailure_IsValidWithWarningDetailAndContent()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "someone@example.com", Nickname = new String('n', 25) };
        var result = validator.GetResult(profile);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content, Is.SameAs(profile));
        Assert.That(result.Detail.Single().Type, Is.EqualTo(ResultType.Warning));
        Assert.That(result.Detail.Single().Property, Is.EqualTo(nameof(Profile.Nickname)));
    }

    [Test]
    public async Task AbstractValidator_GetResultAsyncWithWarningSeverityFailure_IsValidWithWarningDetailAndContent()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "someone@example.com", Nickname = new String('n', 25) };
        var result = await validator.GetResult(profile, CancellationToken.None);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content, Is.SameAs(profile));
        Assert.That(result.Detail.Single().Type, Is.EqualTo(ResultType.Warning));
    }

    [Test]
    public void AbstractValidator_GetResultWithInfoSeverityFailure_IsValidWithWarningDetailAndContent()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "someone@example.com", Nickname = "admin" };
        var result = validator.GetResult(profile);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content, Is.SameAs(profile));
        Assert.That(result.Detail.Single().Type, Is.EqualTo(ResultType.Warning));
    }

    [Test]
    public void AbstractValidator_GetResultWithErrorAndWarningFailures_IsInvalidWithoutContent()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = String.Empty, Nickname = new String('n', 25) };
        var result = validator.GetResult(profile);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Content, Is.Null);
        Assert.That(result.Detail.Select(t => t.Type), Is.EquivalentTo(new[] { ResultType.Error, ResultType.Warning }));
    }

    [Test]
    public void AbstractValidator_GetResultWithErrorSeverityFailure_HasErrorDetail()
    {
        var validator = new StringValidator();
        var result = validator.GetResult(String.Empty);

        Assert.That(result.Detail.Single().Type, Is.EqualTo(ResultType.Error));
    }

    [Test]
    public void AbstractValidator_GetResultWithCustomErrorCode_PreservesErrorCode()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "taken@example.com", Nickname = "someone" };
        var result = validator.GetResult(profile);

        Assert.That(result.HasResultCode("DuplicateEmail"), Is.True);
        Assert.That(result.HasResultCode(ResultCode.InvalidRequestDetail), Is.False);
        Assert.That(result.Detail.Single().Code, Is.EqualTo(new ResultCode("DuplicateEmail")));
    }

    [Test]
    public void AbstractValidator_GetResultWithDefaultErrorCode_UsesFluentValidationErrorCode()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = String.Empty, Nickname = "someone" };
        var result = validator.GetResult(profile);

        Assert.That(result.HasResultCode("NotEmptyValidator"), Is.True);
    }

    [Test]
    public void AbstractValidator_GetResultWithoutErrorCode_UsesInvalidRequestDetail()
    {
        var validator = new ProfileValidator();
        var profile = new Profile { Email = "someone@example.com", Nickname = "anonymous" };
        var result = validator.GetResult(profile);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.HasResultCode(ResultCode.InvalidRequestDetail), Is.True);
    }

    [Test]
    public void AbstractValidator_GetResultWithNullReference_ThrowsException()
    {
        Assert.That(() => AbstractValidatorExtensions.GetResult(null!, String.Empty), Throws.ArgumentNullException);
    }

    [Test]
    public void AbstractValidator_GetResultAsyncWithNullReference_ThrowsException()
    {
        Assert.That(() => AbstractValidatorExtensions.GetResult(null!, String.Empty, CancellationToken.None), Throws.ArgumentNullException);
    }
}
