using MediatR;
using SACS.Application.Authentication.DTOs;

namespace SACS.Application.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
