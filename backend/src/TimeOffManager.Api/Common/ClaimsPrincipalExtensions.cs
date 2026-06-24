using System.Security.Claims;

namespace TimeOffManager.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the authenticated user's id from the token. The API is the only
    /// authority for identity — the client never supplies its own user id.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var id))
            return id;

        throw new InvalidOperationException("The authenticated user has no valid identifier claim.");
    }
}
