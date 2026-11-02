using System.Security.Claims;

namespace Nythorion.Api.Extensions;

internal static class ClaimsPrincipalExtensions
{
    internal static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
}
