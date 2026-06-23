using MediatR;

namespace SACS.Application.Authentication.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
