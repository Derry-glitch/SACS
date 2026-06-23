using MediatR;
using SACS.Application.Authentication.DTOs;

namespace SACS.Application.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;
