using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Authentication;

public class TokenService(
    JwtTokenGenerator jwtTokenGenerator) : ITokenService
{
    public string GenerateAccessToken(
        User user,
        int sessionId,
        DateTime expiresAt)
    {
        return jwtTokenGenerator.GenerateAccessToken(
            user,
            sessionId,
            expiresAt);
    }

    public RefreshToken GenerateRefreshToken(
        int userId,
        DateTime expiresAt)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);

        return new RefreshToken(
            userId,
            token,
            expiresAt);
    }

    public string? GetTokenId(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(accessToken))
            return null;

        var jwt = handler.ReadJwtToken(accessToken);

        return jwt.Claims
            .FirstOrDefault(x =>
                x.Type == JwtRegisteredClaimNames.Jti)
            ?.Value;
    }

    public DateTime? GetTokenExpiration(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(accessToken))
            return null;

        return handler.ReadJwtToken(accessToken).ValidTo;
    }
}