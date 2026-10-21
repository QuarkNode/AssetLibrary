
namespace AssetLibrary.Extensions
{
    using System.Security.Claims;
    public static class ClaimsPrincipalExtension
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

        public static int GetRequiredUserId(this ClaimsPrincipal user)
        {
            return user.GetUserId()
                ?? throw new InvalidOperationException("No authenticated user Id found.");
        }
    }
}
