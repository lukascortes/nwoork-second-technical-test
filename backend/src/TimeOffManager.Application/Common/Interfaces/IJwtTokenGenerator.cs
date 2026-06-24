using TimeOffManager.Domain.Entities;

namespace TimeOffManager.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(User user);
}

/// <summary>Issued access token plus its absolute UTC expiry.</summary>
public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
