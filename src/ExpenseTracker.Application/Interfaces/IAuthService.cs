using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        int userId,
        string refreshToken,
        string? accessToken,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request,
        string? accessToken,
        CancellationToken cancellationToken = default);
}