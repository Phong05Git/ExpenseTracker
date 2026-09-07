using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ITokenBlacklistService tokenBlacklistService) : IAuthService
{
    private const int MaxFailedAttempts = 5;

    private static readonly TimeSpan LockDuration =
        TimeSpan.FromMinutes(15);

    private static readonly TimeSpan AccessTokenLifetime =
        TimeSpan.FromMinutes(15);

    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(30);

    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.GetByUsernameAsync(
                username,
                cancellationToken) != null)
        {
            return Result<AuthResponseDto>.Failure(
                "Username already exists.");
        }

        if (await userRepository.GetByEmailAsync(
                email,
                cancellationToken) != null)
        {
            return Result<AuthResponseDto>.Failure(
                "Email already exists.");
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = new User(
            username,
            passwordHash,
            request.FullName.Trim(),
            email);

        await userRepository.AddAsync(
            user,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await CreateAuthResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var user = await userRepository.GetByUsernameAsync(
            username,
            cancellationToken);

        if (user == null)
        {
            return Result<AuthResponseDto>.Failure(
                "Invalid username or password.");
        }

        if (user.LockUntil.HasValue &&
            user.LockUntil.Value > DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure(
                "Account is temporarily locked.");
        }

        if (!passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            user.IncreaseFailedAttempts();

            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.Lock(
                    DateTime.UtcNow.Add(LockDuration));
            }

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<AuthResponseDto>.Failure(
                "Invalid username or password.");
        }

        user.ResetFailedAttempts();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await CreateAuthResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var oldRefreshToken =
            await refreshTokenRepository.GetByTokenAsync(
                request.RefreshToken,
                cancellationToken);

        if (oldRefreshToken == null ||
            !oldRefreshToken.IsActive)
        {
            return Result<AuthResponseDto>.Failure(
                "Invalid or expired refresh token.");
        }

        var user = await userRepository.GetByIdAsync(
            oldRefreshToken.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result<AuthResponseDto>.Failure(
                "User not found.");
        }

        oldRefreshToken.Revoke();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await CreateAuthResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<Result> LogoutAsync(
        int userId,
        string refreshToken,
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        var storedRefreshToken =
            await refreshTokenRepository.GetByTokenAsync(
                refreshToken,
                cancellationToken);

        if (storedRefreshToken != null &&
            storedRefreshToken.UserId == userId &&
            storedRefreshToken.IsActive)
        {
            storedRefreshToken.Revoke();

            await unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        BlacklistAccessToken(accessToken);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request,
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        if (!passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash))
        {
            return Result.Failure(
                "Current password is incorrect.");
        }

        var passwordHash = passwordHasher.Hash(
            request.NewPassword);

        user.UpdatePasswordHash(passwordHash);

        await refreshTokenRepository.RevokeAllByUserIdAsync(
            userId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        BlacklistAccessToken(accessToken);

        return Result.Success();
    }

    private async Task<Result<AuthResponseDto>> CreateAuthResponseAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var accessTokenExpiresAt =
            DateTime.UtcNow.Add(AccessTokenLifetime);

        var refreshTokenExpiresAt =
            DateTime.UtcNow.Add(RefreshTokenLifetime);

        var refreshToken =
            tokenService.GenerateRefreshToken(
                user.Id,
                refreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var accessToken =
            tokenService.GenerateAccessToken(
                user,
                refreshToken.Id,
                accessTokenExpiresAt);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };

        return Result<AuthResponseDto>.Success(
            response);
    }

    private void BlacklistAccessToken(
        string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        var tokenId =
            tokenService.GetTokenId(accessToken);

        var expiresAt =
            tokenService.GetTokenExpiration(accessToken);

        if (!string.IsNullOrWhiteSpace(tokenId) &&
            expiresAt.HasValue)
        {
            tokenBlacklistService.Revoke(
                tokenId,
                expiresAt.Value);
        }
    }
}