using Application.Abstraction.Mediator;

namespace Application.Auth.Commands.Logout;

public record LogoutRequest(string RefreshToken)
    : IRequest<Unit>, ITransactionalRequest;
