using Application.Abstraction.Mediator;
using Application.Abstraction.Services;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Auth.Commands.Register;

internal sealed class RegisterHandler(
    IUserRepository userRepository,
    ITokenService tokenService
) : IRequestHandler<RegisterRequest, TokenPairDto>
{
    public async Task<TokenPairDto> HandleAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUSer = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUSer != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DateOfBirth = request.DateOfBirth
        };

        var createdUser = userRepository.Add(user);

        var tokens = await tokenService.GenerateTokenPairAsync(createdUser.Id, cancellationToken);

        return tokens;
    }
}