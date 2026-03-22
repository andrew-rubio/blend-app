namespace Blend.Infrastructure.Media;

/// <summary>
/// Builds deterministic blob paths within the <c>blend-media</c> container (per ADR 0007).
/// </summary>
/// <remarks>
/// Path structure:
/// <list type="bullet">
///   <item><description><c>profiles/{userId}/{blobName}</c></description></item>
///   <item><description><c>recipes/{recipeId}/v{version}/{blobName}</c></description></item>
///   <item><description><c>content/{contentId}/{blobName}</c></description></item>
/// </list>
/// The caller determines the <paramref name="blobName"/>, which includes the file extension.
/// </remarks>
public static class BlobPathBuilder
{
    /// <summary>Returns the blob path for a profile photo.</summary>
    public static string ForProfile(string userId, string blobName) =>
        $"profiles/{userId}/{blobName}";

    /// <summary>Returns the blob path for a recipe image.</summary>
    public static string ForRecipe(string recipeId, string version, string blobName) =>
        $"recipes/{recipeId}/{version}/{blobName}";

    /// <summary>Returns the blob path for an admin content image.</summary>
    public static string ForContent(string contentId, string blobName) =>
        $"content/{contentId}/{blobName}";

    /// <summary>
    /// Derives the folder prefix (directory) from an existing blob path.
    /// For example, <c>profiles/user-1/original.jpg</c> → <c>profiles/user-1/</c>.
    /// </summary>
    public static string GetFolder(string blobPath)
    {
        var lastSlash = blobPath.LastIndexOf('/');
        return lastSlash < 0 ? string.Empty : blobPath[..(lastSlash + 1)];
    }
}
