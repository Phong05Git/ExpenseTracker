using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.API.Middleware;

public class SessionActivityMiddleware(
    RequestDelegate next,
    ITokenBlacklistService tokenBlacklistService,
    IConfiguration configuration)
{
    private static readonly TimeSpan DefaultIdleTimeout =
        TimeSpan.FromMinutes(30);

    private static readonly TimeSpan DefaultActivityUpdateInterval =
        TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(
        HttpContext context,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var sessionIdText =
            context.User.FindFirstValue(JwtRegisteredClaimNames.Sid);

        var tokenId =
            context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (!int.TryParse(
                sessionIdText,
                out var sessionId) ||
            string.IsNullOrWhiteSpace(tokenId))
        {
            await RejectAsync(
                context,
                "Invalid session information.");

            return;
        }

        var session =
            await refreshTokenRepository.GetByIdAsync(
                sessionId,
                context.RequestAborted);

        if (session == null || !session.IsActive)
        {
            await RejectAsync(
                context,
                "Session is no longer active.");

            return;
        }

        var nowUtc = DateTime.UtcNow;

        var idleTimeoutMinutes =
            configuration.GetValue<int?>(
                "Security:IdleTimeoutMinutes")
            ?? (int)DefaultIdleTimeout.TotalMinutes;

        var updateIntervalMinutes =
            configuration.GetValue<int?>(
                "Security:ActivityUpdateIntervalMinutes")
            ?? (int)DefaultActivityUpdateInterval.TotalMinutes;

        var idleTimeout =
            TimeSpan.FromMinutes(idleTimeoutMinutes);

        var updateInterval =
            TimeSpan.FromMinutes(updateIntervalMinutes);

        if (nowUtc - session.LastActivityAt > idleTimeout)
        {
            session.Revoke();

            await unitOfWork.SaveChangesAsync(context.RequestAborted);

            var expiration = GetTokenExpiration(context);

            if (expiration.HasValue)
            {
                tokenBlacklistService.Revoke(
                    tokenId,
                    expiration.Value);
            }

            await RejectAsync(
                context,
                "Session expired because of inactivity.");

            return;
        }

        if (nowUtc - session.LastActivityAt >= updateInterval)
        {
            session.UpdateLastActivity(nowUtc);

            await unitOfWork.SaveChangesAsync(context.RequestAborted);
        }

        await next(context);
    }

    private static DateTime? GetTokenExpiration(HttpContext context)
    {
        var expirationText = context.User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (!long.TryParse(
                expirationText,
                out var expirationUnix))
        {
            return null;
        }

        return DateTimeOffset
            .FromUnixTimeSeconds(expirationUnix)
            .UtcDateTime;
    }

    private static async Task RejectAsync(
        HttpContext context,
        string detail)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.ContentType =
            "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                status = StatusCodes.Status401Unauthorized,
                title = "Unauthorized.",
                detail
            });
    }
}