using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Validators;
using FluentValidation.Results;

namespace ExpenseTracker.UnitTests;

public class AuthRequestValidatorTests
{
    private const string ValidPassword = "Test-123!";

    public static TheoryData<string, bool> UsernameInputs => new()
    {
        { "", false }, { "   ", false }, { "ab", false }, { "abc", true },
        { new string('a', 50), true }, { new string('a', 51), false }
    };

    public static TheoryData<string?, bool> ExistingPasswordInputs => new()
    {
        { null, false }, { "", false }, { "   ", false }, { "x", true },
        { new string('a', 71), true }, { new string('a', 72), true }, { new string('a', 73), false },
        { new string('é', 36), true }, { new string('é', 37), false },
        { "fixture\0", false }, { " e\u0301🙂 ", true }
    };

    public static TheoryData<string, bool> NewPasswordInputs => new()
    {
        { "", false }, { "        ", false },
        { "Aa1!aaa", false }, { "Aa1!aaaa", true },
        { "Aa1!" + new string('a', 67), true },
        { "Aa1!" + new string('a', 68), true },
        { "Aa1!" + new string('a', 69), false },
        { "Aa1!a" + new string('é', 33), true },
        { "Aa1!" + new string('é', 34), true },
        { "Aa1!a" + new string('é', 34), false },
        { "Aa1!fixture\0", false },
        { "AAAA123!", false }, { "aaaa123!", false }, { "Aaaaabc!", false }, { "Aaaa1234", false }
    };

    public static TheoryData<string, bool> FullNameInputs => new()
    {
        { "", false }, { "   ", false }, { "Test User", true },
        { new string('a', 100), true }, { new string('a', 101), false }
    };

    public static TheoryData<string, bool> EmailInputs => new()
    {
        { "", false }, { "   ", false }, { "missing-at.example.invalid", false },
        { "test@example.invalid", true },
        { LongEmail(58), true }, { LongEmail(59), false }
    };

    private static string LongEmail(int labelLength) =>
        new string('a', 64) + "@" + new string('b', 63) + "." + new string('c', 63) +
        "." + new string('d', labelLength) + ".com";

    [Theory]
    [MemberData(nameof(UsernameInputs))]
    public void Login_ValidatesUsernamePresenceAndLength(string username, bool valid)
    {
        // BE-UT.03-09
        var request = new LoginRequestDto { Username = username, Password = ValidPassword };

        var result = new LoginRequestValidator().Validate(request);

        AssertField(result, nameof(LoginRequestDto.Username), valid);
    }

    [Theory]
    [MemberData(nameof(ExistingPasswordInputs))]
    public void Login_ValidatesExistingPasswordPresenceAndBytePolicy(string? password, bool valid)
    {
        // BE-UT.03-10
        var request = new LoginRequestDto { Username = "test-user", Password = password! };

        var result = new LoginRequestValidator().Validate(request);

        AssertField(result, nameof(LoginRequestDto.Password), valid);
    }

    [Theory]
    [MemberData(nameof(UsernameInputs))]
    public void Register_ValidatesUsernamePresenceAndLength(string username, bool valid)
    {
        // BE-UT.03-11
        var request = Registration(username: username);

        var result = new RegisterRequestValidator().Validate(request);

        AssertField(result, nameof(RegisterRequestDto.Username), valid);
    }

    [Theory]
    [MemberData(nameof(FullNameInputs))]
    public void Register_ValidatesFullNamePresenceAndMaximumLength(string fullName, bool valid)
    {
        // BE-UT.03-12
        var request = Registration(fullName: fullName);

        var result = new RegisterRequestValidator().Validate(request);

        AssertField(result, nameof(RegisterRequestDto.FullName), valid);
    }

    [Theory]
    [MemberData(nameof(EmailInputs))]
    public void Register_ValidatesEmailPresenceFormatAndMaximumLength(string email, bool valid)
    {
        // BE-UT.03-13
        var request = Registration(email: email);

        var result = new RegisterRequestValidator().Validate(request);

        AssertField(result, nameof(RegisterRequestDto.Email), valid);
    }

    [Theory]
    [MemberData(nameof(NewPasswordInputs))]
    public void Register_AppliesPasswordLengthAndComplexity(string password, bool valid)
    {
        // BE-UT.03-14
        var request = Registration(password: password);

        var result = new RegisterRequestValidator().Validate(request);

        AssertField(result, nameof(RegisterRequestDto.Password), valid);
    }

    [Theory]
    [MemberData(nameof(ExistingPasswordInputs))]
    public void ChangePassword_ValidatesCurrentPasswordPresenceAndBytePolicy(string? password, bool valid)
    {
        // BE-UT.03-15
        var request = new ChangePasswordRequestDto { CurrentPassword = password!, NewPassword = ValidPassword };

        var result = new ChangePasswordValidator().Validate(request);

        AssertField(result, nameof(ChangePasswordRequestDto.CurrentPassword), valid);
    }

    [Theory]
    [MemberData(nameof(NewPasswordInputs))]
    public void ChangePassword_AppliesNewPasswordLengthAndComplexity(string password, bool valid)
    {
        // BE-UT.03-16
        var request = new ChangePasswordRequestDto { CurrentPassword = "previous-fixture", NewPassword = password };

        var result = new ChangePasswordValidator().Validate(request);

        AssertField(result, nameof(ChangePasswordRequestDto.NewPassword), valid);
    }

    [Fact]
    public void ExistingPasswordValidators_RejectUnpairedSurrogates()
    {
        const string password = "fixture\uD800";
        var login = new LoginRequestValidator().Validate(
            new LoginRequestDto { Username = "test-user", Password = password });
        var change = new ChangePasswordValidator().Validate(
            new ChangePasswordRequestDto { CurrentPassword = password, NewPassword = ValidPassword });

        AssertField(login, nameof(LoginRequestDto.Password), false);
        AssertField(change, nameof(ChangePasswordRequestDto.CurrentPassword), false);
    }

    [Fact]
    public void NewPasswordValidators_RejectUnpairedSurrogates()
    {
        const string password = "Aa1!fixture\uD800";
        var registration = new RegisterRequestValidator().Validate(Registration(password: password));
        var change = new ChangePasswordValidator().Validate(
            new ChangePasswordRequestDto { CurrentPassword = ValidPassword, NewPassword = password });

        AssertField(registration, nameof(RegisterRequestDto.Password), false);
        AssertField(change, nameof(ChangePasswordRequestDto.NewPassword), false);
    }

    [Fact]
    public void ChangePassword_RejectsReusingCurrentPassword()
    {
        // BE-UT.03-17
        var request = new ChangePasswordRequestDto { CurrentPassword = ValidPassword, NewPassword = ValidPassword };

        var result = new ChangePasswordValidator().Validate(request);

        AssertField(result, nameof(ChangePasswordRequestDto.NewPassword), false);
    }

    private static RegisterRequestDto Registration(string username = "test-user", string fullName = "Test User",
        string email = "test@example.invalid", string password = ValidPassword) =>
        new() { Username = username, FullName = fullName, Email = email, Password = password };

    private static void AssertField(ValidationResult result, string property, bool valid)
    {
        Assert.Equal(valid, result.IsValid);
        if (!valid)
        {
            Assert.NotEmpty(result.Errors);
            Assert.All(result.Errors, failure => Assert.Equal(property, failure.PropertyName));
        }
    }
}
