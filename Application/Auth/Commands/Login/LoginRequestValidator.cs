using FluentValidation;

namespace Application.Auth.Commands.Login;

public sealed class LoginRequestValidator
    : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
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
    }
}