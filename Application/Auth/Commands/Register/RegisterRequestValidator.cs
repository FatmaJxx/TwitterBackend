using FluentValidation;

namespace Application.Auth.Commands.Register;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(225);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"(?:.*[0-9]){2}")
            .WithMessage("Password must contain at least two digits.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(request => request.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth <=
                                 DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-20))
            .WithMessage("You must be at least 20 years old.");
    }
}